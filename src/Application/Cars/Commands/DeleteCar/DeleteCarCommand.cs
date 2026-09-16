using MediatR;

namespace CarRental.Application.Cars.Commands.DeleteCar;

public record DeleteCarCommand(int CarId) : IRequest;
