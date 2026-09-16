using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Rentals.Commands.RegisterRental;

public class RegisterRentalCommandHandler(
    ICustomerRepository customerRepository,
    ICarRepository carRepository,
    IRentalRepository rentalRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterRentalCommand, RentalDto>
{
    public async Task<RentalDto> Handle(RegisterRentalCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        var car = await carRepository.GetByIdAsync(request.CarId, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.CarId);

        var hasOverlap = await rentalRepository.HasOverlapAsync(
            car.Id, request.StartDate, request.EndDate, excludeRentalId: null, cancellationToken);

        if (hasOverlap)
            throw new CarNotAvailableException(car.Id, request.StartDate, request.EndDate);

        var rental = new Rental
        {
            Customer = customer,
            CustomerId = customer.Id,
            Car = car,
            CarId = car.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = RentalStatus.Active
        };

        await rentalRepository.AddAsync(rental, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rental.ToDto();
    }
}
