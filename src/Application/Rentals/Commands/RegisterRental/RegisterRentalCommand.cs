using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Rentals.Commands.RegisterRental;

public record RegisterRentalCommand(int CustomerId, int CarId, DateTime StartDate, DateTime EndDate)
    : IRequest<RentalDto>;
