using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Cars.Commands.DeleteCar;

public class DeleteCarCommandHandler(
    ICarRepository carRepository,
    IRentalRepository rentalRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCarCommand>
{
    public async Task Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        var car = await carRepository.GetByIdAsync(request.CarId, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.CarId);

        if (await rentalRepository.ExistsForCarAsync(car.Id, cancellationToken))
            throw new CarInUseException(car.Id);

        carRepository.Remove(car);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
