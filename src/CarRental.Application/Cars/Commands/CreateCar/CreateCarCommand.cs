using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Commands.CreateCar;

public record CreateCarCommand(string Type, string Model) : IRequest<CarDto>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["cars"];
}
