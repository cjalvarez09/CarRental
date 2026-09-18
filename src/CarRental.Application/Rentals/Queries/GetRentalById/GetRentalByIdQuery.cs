using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Rentals.Queries.GetRentalById;

public record GetRentalByIdQuery(int RentalId) : IRequest<RentalDto>, ICacheableQuery
{
    public string CacheKey => $"rentals:{RentalId}";

    public IReadOnlyCollection<string> CacheTags => ["rentals"];
}
