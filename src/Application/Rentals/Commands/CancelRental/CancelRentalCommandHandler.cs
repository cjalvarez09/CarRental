using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Rentals.Commands.CancelRental;

public class CancelRentalCommandHandler(IRentalRepository rentalRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelRentalCommand>
{
    public async Task Handle(CancelRentalCommand request, CancellationToken cancellationToken)
    {
        var rental = await rentalRepository.GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new NotFoundException(nameof(Rental), request.RentalId);

        if (rental.Status == RentalStatus.Cancelled)
            throw new DomainException("Rental is already cancelled.");

        rental.Status = RentalStatus.Cancelled;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
