using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Password,
    string Role,
    string? FullName,
    string? Address,
    string? Email) : IRequest<UserDto>;
