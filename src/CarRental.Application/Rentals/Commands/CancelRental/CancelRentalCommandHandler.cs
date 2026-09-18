using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Rentals.Commands.CancelRental;

public class CancelRentalCommandHandler(
    IRentalRepository rentalRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelRentalCommand>
{
    public async Task Handle(CancelRentalCommand request, CancellationToken cancellationToken)
    {
        var rental = await rentalRepository.GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new NotFoundException(nameof(Rental), request.RentalId);

        var isCustomer = string.Equals(currentUserService.Role, Roles.Customer, StringComparison.Ordinal);

        // A customer can only touch their own rentals; for anyone else's it looks like it doesn't exist.
        if (isCustomer && rental.CustomerId != currentUserService.CustomerId)
            throw new NotFoundException(nameof(Rental), request.RentalId);

        if (rental.Status == RentalStatus.Cancelled)
            throw new DomainException("Rental is already cancelled.");

        // Employees can cancel at any time, customers only before the rental starts.
        if (isCustomer && rental.StartDate.Date <= DateTime.UtcNow.Date)
            throw new RentalInProgressException(rental.Id);

        rental.Status = RentalStatus.Cancelled;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
