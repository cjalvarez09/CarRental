using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Rentals.Commands.ModifyRental;

public class ModifyRentalCommandHandler(IRentalRepository rentalRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ModifyRentalCommand, RentalDto>
{
    public async Task<RentalDto> Handle(ModifyRentalCommand request, CancellationToken cancellationToken)
    {
        var rental = await rentalRepository.GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new NotFoundException(nameof(Rental), request.RentalId);

        if (rental.Status == RentalStatus.Cancelled)
            throw new DomainException("Cannot reschedule a cancelled rental.");

        var hasOverlap = await rentalRepository.HasOverlapAsync(
            rental.CarId, request.StartDate, request.EndDate, excludeRentalId: rental.Id, cancellationToken);

        if (hasOverlap)
            throw new CarNotAvailableException(rental.CarId, request.StartDate, request.EndDate);

        rental.StartDate = request.StartDate;
        rental.EndDate = request.EndDate;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rental.ToDto();
    }
}
