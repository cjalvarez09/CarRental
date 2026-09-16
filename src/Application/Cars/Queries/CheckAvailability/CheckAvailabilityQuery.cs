using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Queries.CheckAvailability;

public record CheckAvailabilityQuery(string Type, string? Model, DateTime StartDate, DateTime EndDate)
    : IRequest<IReadOnlyList<CarDto>>;
