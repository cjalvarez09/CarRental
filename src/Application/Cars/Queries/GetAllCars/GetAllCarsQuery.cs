using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Queries.GetAllCars;

public record GetAllCarsQuery : IRequest<IReadOnlyList<CarDto>>;
