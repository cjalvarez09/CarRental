using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Queries.GetCarById;

public record GetCarByIdQuery(int CarId) : IRequest<CarDto>, ICacheableQuery
{
    public string CacheKey => $"cars:{CarId}";

    public IReadOnlyCollection<string> CacheTags => ["cars"];
}
