using FluentValidation;

namespace CarRental.Application.Cars.Commands.DeleteCar;

public class DeleteCarCommandValidator : AbstractValidator<DeleteCarCommand>
{
    public DeleteCarCommandValidator()
    {
        RuleFor(command => command.CarId).GreaterThan(0);
    }
}
