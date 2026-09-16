using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Rentals.Queries.GetRentalById;

public record GetRentalByIdQuery(int RentalId) : IRequest<RentalDto>;
