using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int CustomerId) : IRequest<CustomerDto>, ICacheableQuery
{
    public string CacheKey => $"customers:{CustomerId}";

    public IReadOnlyCollection<string> CacheTags => ["customers"];
}
