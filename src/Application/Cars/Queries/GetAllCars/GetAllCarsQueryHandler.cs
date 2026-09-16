using CarRental.Application.Common.Models;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Cars.Queries.GetAllCars;

public class GetAllCarsQueryHandler(ICarRepository carRepository)
    : IRequestHandler<GetAllCarsQuery, IReadOnlyList<CarDto>>
{
    public async Task<IReadOnlyList<CarDto>> Handle(GetAllCarsQuery request, CancellationToken cancellationToken)
    {
        var cars = await carRepository.GetAllAsync(cancellationToken);

        return [.. cars.Select(car => car.ToDto())];
    }
}
