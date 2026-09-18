using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Customers.Commands.RegisterCustomer;

public class RegisterCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        // This only creates the Customer profile, with no linked User (unlike self-registration
        // via /api/auth/register). In a real system this should also generate a temporary password
        // and email the customer an activation link so they can claim a login for this profile.
        var customer = new Customer
        {
            FullName = request.FullName,
            Address = request.Address,
            Email = request.Email
        };

        await customerRepository.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }
}
