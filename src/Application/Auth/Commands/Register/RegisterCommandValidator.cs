using CarRental.Domain.Enums;
using FluentValidation;

namespace CarRental.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Username).NotEmpty().MinimumLength(3);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(6);
        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be either 'Customer' or 'Employee'.");

        When(command => IsCustomerRole(command.Role), () =>
        {
            RuleFor(command => command.FullName).NotEmpty();
            RuleFor(command => command.Address).NotEmpty();
            RuleFor(command => command.Email).NotEmpty().EmailAddress();
        });
    }

    private static bool IsCustomerRole(string role) =>
        Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed) && parsed == UserRole.Customer;
}
