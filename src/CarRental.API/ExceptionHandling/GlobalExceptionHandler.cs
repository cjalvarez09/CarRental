using CarRental.Application.Common.Exceptions;
using CarRental.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.ExceptionHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
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

        // Write the JSON directly instead of relying on IProblemDetailsService's content
        // negotiation against the Accept header, which can silently fail to match (e.g.
        // some browser-originated fetches) and fall back to a bodyless generic response.
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
        DomainException => (StatusCodes.Status409Conflict, "Business rule violation"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
    };
}
