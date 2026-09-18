using FluentValidation;

namespace CarRental.Application.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(command => command.CustomerId).GreaterThan(0);
        RuleFor(command => command.FullName).NotEmpty();
        RuleFor(command => command.Address).NotEmpty();
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
