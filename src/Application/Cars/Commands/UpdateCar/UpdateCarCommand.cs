using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Commands.UpdateCar;

public record UpdateCarCommand(int CarId, string Type, string Model) : IRequest<CarDto>;
