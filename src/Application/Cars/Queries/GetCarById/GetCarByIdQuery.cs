using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Queries.GetCarById;

public record GetCarByIdQuery(int CarId) : IRequest<CarDto>;
