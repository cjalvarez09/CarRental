using FluentValidation;

namespace CarRental.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Username).NotEmpty().MinimumLength(3);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(6);
    }
}
