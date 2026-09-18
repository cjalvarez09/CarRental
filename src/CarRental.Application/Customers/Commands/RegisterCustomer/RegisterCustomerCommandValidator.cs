using FluentValidation;

namespace CarRental.Application.Customers.Commands.RegisterCustomer;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(command => command.FullName).NotEmpty();
        RuleFor(command => command.Address).NotEmpty();
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
