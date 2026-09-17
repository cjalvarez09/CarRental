using CarRental.Application.Common.Caching;
using MediatR;

namespace CarRental.Application.Rentals.Commands.CancelRental;

public record CancelRentalCommand(int RentalId) : IRequest, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["rentals", "cars"];
}
