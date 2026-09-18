using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(int CustomerId, string FullName, string Address, string Email)
    : IRequest<CustomerDto>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["customers"];
}
