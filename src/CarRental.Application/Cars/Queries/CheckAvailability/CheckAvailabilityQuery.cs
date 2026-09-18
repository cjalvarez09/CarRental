using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Queries.CheckAvailability;

public record CheckAvailabilityQuery(string Type, string? Model, DateTime StartDate, DateTime EndDate)
    : IRequest<IReadOnlyList<CarDto>>, ICacheableQuery
{
    public string CacheKey => $"cars:availability:{Type}:{Model}:{StartDate:yyyyMMdd}:{EndDate:yyyyMMdd}";

    public IReadOnlyCollection<string> CacheTags => ["cars"];
}
