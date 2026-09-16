using MediatR;

namespace CarRental.Application.Rentals.Commands.CancelRental;

public record CancelRentalCommand(int RentalId) : IRequest;
