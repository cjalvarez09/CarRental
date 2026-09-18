using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Cars.Queries.GetCarById;

public class GetCarByIdQueryHandler(ICarRepository carRepository)
    : IRequestHandler<GetCarByIdQuery, CarDto>
{
    public async Task<CarDto> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        var car = await carRepository.GetByIdAsync(request.CarId, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.CarId);

        return car.ToDto();
    }
}
