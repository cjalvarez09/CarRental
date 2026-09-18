using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Queries.GetAllCars;

public record GetAllCarsQuery : IRequest<IReadOnlyList<CarDto>>, ICacheableQuery
{
    public string CacheKey => "cars:all";

    public IReadOnlyCollection<string> CacheTags => ["cars"];
}
