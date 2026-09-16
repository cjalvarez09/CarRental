using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Cars.Commands.CreateCar;

public record CreateCarCommand(string Type, string Model) : IRequest<CarDto>;
