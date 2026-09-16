using CarRental.Application.Common.Models;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetAllCustomersQuery, IReadOnlyList<CustomerDto>>
{
    public async Task<IReadOnlyList<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);

        return [.. customers.Select(customer => customer.ToDto())];
    }
}
