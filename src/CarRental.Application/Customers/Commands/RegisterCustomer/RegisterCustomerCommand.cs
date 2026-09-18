using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Commands.RegisterCustomer;

public record RegisterCustomerCommand(string FullName, string Address, string Email)
    : IRequest<CustomerDto>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["customers"];
}
