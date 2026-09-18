using System.Text.Json;
using CarRental.API.ExceptionHandling;
using CarRental.Application.Common.Exceptions;
using CarRental.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CarRental.UnitTests.API;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    private static DefaultHttpContext NewContext(string? acceptHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        if (acceptHeader is not null)
            context.Request.Headers.Accept = acceptHeader;

        return context;
    }

    private static JsonElement ReadBody(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return JsonDocument.Parse(context.Response.Body).RootElement;
    }

    public static TheoryData<Exception, int, string> ExceptionMappings => new()
    {
        { new ValidationException([new ValidationFailure("Name", "required")]), 400, "Validation failed" },
        { new NotFoundException("Car", 1), 404, "Resource not found" },
        { new InvalidCredentialsException(), 401, "Invalid credentials" },
        { new UsernameAlreadyExistsException("juan"), 409, "Business rule violation" },
        { new CarNotAvailableException(1, new DateTime(2030, 1, 1), new DateTime(2030, 1, 5)), 409, "Business rule violation" },
        { new CarInUseException(1), 409, "Business rule violation" },
        { new CustomerInUseException(1), 409, "Business rule violation" },
        { new DomainException("generic rule broken"), 409, "Business rule violation" },
        { new InvalidOperationException("boom"), 500, "An unexpected error occurred" }
    };

    [Theory]
    [MemberData(nameof(ExceptionMappings))]
    public async Task Given_AnException_When_Handling_Then_MapsItToTheExpectedStatusAndTitle(
        Exception exception, int expectedStatus, string expectedTitle)
    {
        // Given
        var context = NewContext();

        // When
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Then
        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        var body = ReadBody(context);
        Assert.Equal(expectedStatus, body.GetProperty("status").GetInt32());
        Assert.Equal(expectedTitle, body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Given_AKnownException_When_Handling_Then_ExposesItsMessageAsDetail()
    {
        // Given
        var context = NewContext();

        // When
        await _handler.TryHandleAsync(context, new NotFoundException("Car", 7), CancellationToken.None);

        // Then
        Assert.Equal("Car with id '7' was not found.", ReadBody(context).GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Given_AnUnexpectedException_When_Handling_Then_HidesTheInternalMessage()
    {
        // Given
        var context = NewContext();

        // When
        await _handler.TryHandleAsync(context, new InvalidOperationException("connection string is xyz"), CancellationToken.None);

        // Then
        var detail = ReadBody(context).GetProperty("detail").GetString();
        Assert.DoesNotContain("xyz", detail);
        Assert.Equal("An unexpected error occurred. Please try again later.", detail);
    }

    [Fact]
    public async Task Given_AValidationException_When_Handling_Then_IncludesErrorsGroupedByProperty()
    {
        // Given
        var context = NewContext();
        var exception = new ValidationException(
        [
            new ValidationFailure("Email", "is required"),
            new ValidationFailure("Email", "is invalid"),
            new ValidationFailure("Name", "is required")
        ]);

        // When
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Then
        var errors = ReadBody(context).GetProperty("errors");
        Assert.Equal(["is required", "is invalid"], errors.GetProperty("Email").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal(["is required"], errors.GetProperty("Name").EnumerateArray().Select(e => e.GetString()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("*/*")]
    [InlineData("text/plain")]
    [InlineData("text/html")]
    public async Task Given_AnyAcceptHeader_When_Handling_Then_AlwaysWritesProblemJson(string? acceptHeader)
    {
        // Given
        var context = NewContext(acceptHeader);

        // When
        var handled = await _handler.TryHandleAsync(context, new DomainException("rule broken"), CancellationToken.None);

        // Then
        Assert.True(handled);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        Assert.Equal("rule broken", ReadBody(context).GetProperty("detail").GetString());
    }
}
