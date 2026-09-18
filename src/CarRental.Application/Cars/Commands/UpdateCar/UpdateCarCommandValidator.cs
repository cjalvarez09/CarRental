using FluentValidation;

namespace CarRental.Application.Cars.Commands.UpdateCar;

public class UpdateCarCommandValidator : AbstractValidator<UpdateCarCommand>
{
    public UpdateCarCommandValidator()
    {
        RuleFor(command => command.CarId).GreaterThan(0);
        RuleFor(command => command.Type).NotEmpty();
        RuleFor(command => command.Model).NotEmpty();
    }
}
