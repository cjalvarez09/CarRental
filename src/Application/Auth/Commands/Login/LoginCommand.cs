using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<AuthResultDto>;
