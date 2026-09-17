using CarRental.Application.Common.Caching;
using MediatR;

namespace CarRental.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int CustomerId) : IRequest, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["customers"];
}
