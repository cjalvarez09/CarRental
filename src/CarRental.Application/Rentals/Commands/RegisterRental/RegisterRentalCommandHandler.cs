using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
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
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterRentalCommand, RentalDto>
{
    public async Task<RentalDto> Handle(RegisterRentalCommand request, CancellationToken cancellationToken)
    {
        var customerId = ResolveCustomerId(request.CustomerId);

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

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

    private int ResolveCustomerId(int requestedCustomerId)
    {
        if (!string.Equals(currentUserService.Role, Roles.Customer, StringComparison.Ordinal))
            return requestedCustomerId;

        // A Customer can only ever book for themselves - ignore whatever CustomerId the
        // request body carries and use the one linked to their own account instead.
        return currentUserService.CustomerId
            ?? throw new DomainException("Your account is not linked to a customer profile.");
    }
}
