using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery : IRequest<IReadOnlyList<CustomerDto>>, ICacheableQuery
{
    public string CacheKey => "customers:all";

    public IReadOnlyCollection<string> CacheTags => ["customers"];
}
