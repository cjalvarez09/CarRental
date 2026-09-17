using CarRental.Application.Common.Caching;
using MediatR;

namespace CarRental.Application.Cars.Commands.DeleteCar;

public record DeleteCarCommand(int CarId) : IRequest, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["cars"];
}
