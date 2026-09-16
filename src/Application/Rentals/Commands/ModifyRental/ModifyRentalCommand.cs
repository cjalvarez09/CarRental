using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Rentals.Commands.ModifyRental;

public record ModifyRentalCommand(int RentalId, DateTime StartDate, DateTime EndDate) : IRequest<RentalDto>;
