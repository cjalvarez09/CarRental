using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        return customer.ToDto();
    }
}
