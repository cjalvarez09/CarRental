using FluentValidation;

namespace CarRental.Application.Rentals.Commands.CancelRental;

public class CancelRentalCommandValidator : AbstractValidator<CancelRentalCommand>
{
    public CancelRentalCommandValidator()
    {
        RuleFor(command => command.RentalId).GreaterThan(0);
    }
}
