using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Cars.Commands.CreateCar;

public class CreateCarCommandHandler(ICarRepository carRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCarCommand, CarDto>
{
    public async Task<CarDto> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var car = new Car
        {
            Type = request.Type,
            Model = request.Model
        };

        await carRepository.AddAsync(car, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return car.ToDto();
    }
}
