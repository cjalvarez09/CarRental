using FluentValidation;

namespace CarRental.Application.Cars.Commands.CreateCar;

public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(command => command.Type).NotEmpty();
        RuleFor(command => command.Model).NotEmpty();
    }
}
