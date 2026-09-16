using FluentValidation;

namespace CarRental.Application.Rentals.Commands.RegisterRental;

public class RegisterRentalCommandValidator : AbstractValidator<RegisterRentalCommand>
{
    public RegisterRentalCommandValidator()
    {
        RuleFor(command => command.CustomerId).GreaterThan(0);
        RuleFor(command => command.CarId).GreaterThan(0);
        RuleFor(command => command.StartDate.Date)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow.Date)
            .WithMessage("Rental start date cannot be in the past.");
        RuleFor(command => command.EndDate)
            .GreaterThan(command => command.StartDate)
            .WithMessage("Rental end date must be after the start date.");
    }
}
