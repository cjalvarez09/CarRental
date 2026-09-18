using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Cars.Commands.UpdateCar;

public class UpdateCarCommandHandler(ICarRepository carRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCarCommand, CarDto>
{
    public async Task<CarDto> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var car = await carRepository.GetByIdAsync(request.CarId, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.CarId);

        car.Type = request.Type;
        car.Model = request.Model;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return car.ToDto();
    }
}
