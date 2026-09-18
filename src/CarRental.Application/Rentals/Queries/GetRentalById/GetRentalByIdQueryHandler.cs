using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Rentals.Queries.GetRentalById;

public class GetRentalByIdQueryHandler(IRentalRepository rentalRepository)
    : IRequestHandler<GetRentalByIdQuery, RentalDto>
{
    public async Task<RentalDto> Handle(GetRentalByIdQuery request, CancellationToken cancellationToken)
    {
        var rental = await rentalRepository.GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new NotFoundException(nameof(Rental), request.RentalId);

        return rental.ToDto();
    }
}
