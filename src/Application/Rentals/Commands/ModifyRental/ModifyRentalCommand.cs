using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Rentals.Commands.ModifyRental;

public record ModifyRentalCommand(int RentalId, DateTime StartDate, DateTime EndDate)
    : IRequest<RentalDto>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["rentals", "cars"];
}
