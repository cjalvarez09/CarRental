using CarRental.Application.Common.Exceptions;
using CarRental.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.ExceptionHandling;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception occurred.");
        else
            logger.LogWarning(exception, "Request failed with status {StatusCode}.", statusCode);

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message
        };

        if (exception is FluentValidation.ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        DomainException => (StatusCodes.Status409Conflict, "Business rule violation"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
    };
}
