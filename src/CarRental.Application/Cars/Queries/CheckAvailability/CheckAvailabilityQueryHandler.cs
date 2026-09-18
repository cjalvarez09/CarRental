using CarRental.Application.Common.Models;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Cars.Queries.CheckAvailability;

public class CheckAvailabilityQueryHandler(ICarRepository carRepository)
    : IRequestHandler<CheckAvailabilityQuery, IReadOnlyList<CarDto>>
{
    public async Task<IReadOnlyList<CarDto>> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var cars = await carRepository.GetAvailableAsync(
            request.Type,
            request.Model,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return [.. cars.Select(car => car.ToDto())];
    }
}
