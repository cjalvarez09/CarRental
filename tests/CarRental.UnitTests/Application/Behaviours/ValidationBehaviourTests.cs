using CarRental.Application.Common.Behaviours;
using FluentValidation;
using MediatR;

namespace CarRental.UnitTests.Application.Behaviours;

public class ValidationBehaviourTests
{
    public record SampleRequest(string Name) : IRequest<string>;

    private sealed class NameRequiredValidator : AbstractValidator<SampleRequest>
    {
        public NameRequiredValidator() => RuleFor(request => request.Name).NotEmpty();
    }

    private sealed class NameMinLengthValidator : AbstractValidator<SampleRequest>
    {
        public NameMinLengthValidator() => RuleFor(request => request.Name).MinimumLength(3);
    }

    private static (RequestHandlerDelegate<string> Next, Func<bool> WasCalled) TrackedNext()
    {
        var called = false;
        return (() =>
        {
            called = true;
            return Task.FromResult("handled");
        }, () => called);
    }

    [Fact]
    public async Task Given_NoValidatorsRegistered_When_Handling_Then_CallsNext()
    {
        // Given
        var behaviour = new ValidationBehaviour<SampleRequest, string>([]);
        var (next, wasCalled) = TrackedNext();

        // When
        var result = await behaviour.Handle(new SampleRequest(""), next, CancellationToken.None);

        // Then
        Assert.Equal("handled", result);
        Assert.True(wasCalled());
    }

    [Fact]
    public async Task Given_AValidRequest_When_Handling_Then_CallsNext()
    {
        // Given
        var behaviour = new ValidationBehaviour<SampleRequest, string>([new NameRequiredValidator()]);
        var (next, wasCalled) = TrackedNext();

        // When
        var result = await behaviour.Handle(new SampleRequest("Juan"), next, CancellationToken.None);

        // Then
        Assert.Equal("handled", result);
        Assert.True(wasCalled());
    }

    [Fact]
    public async Task Given_AnInvalidRequest_When_Handling_Then_ThrowsValidationExceptionWithoutCallingNext()
    {
        // Given
        var behaviour = new ValidationBehaviour<SampleRequest, string>([new NameRequiredValidator()]);
        var (next, wasCalled) = TrackedNext();

        // When
        Func<Task> act = () => behaviour.Handle(new SampleRequest(""), next, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Equal(nameof(SampleRequest.Name), Assert.Single(exception.Errors).PropertyName);
        Assert.False(wasCalled());
    }

    [Fact]
    public async Task Given_SeveralFailingValidators_When_Handling_Then_ReportsTheFailuresOfAllOfThemOnce()
    {
        // Given
        var behaviour = new ValidationBehaviour<SampleRequest, string>(
            [new NameRequiredValidator(), new NameMinLengthValidator()]);
        var (next, wasCalled) = TrackedNext();

        // When
        Func<Task> act = () => behaviour.Handle(new SampleRequest(""), next, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Equal(2, exception.Errors.Count());
        Assert.False(wasCalled());
    }
}
