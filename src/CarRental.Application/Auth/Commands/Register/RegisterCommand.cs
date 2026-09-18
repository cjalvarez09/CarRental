using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Password,
    string Role,
    string? FullName,
    string? Address,
    string? Email) : IRequest<UserDto>, ICacheInvalidatingCommand
{
    // Registering a Customer also creates its Customer profile.
    public IReadOnlyCollection<string> CacheTagsToInvalidate => ["customers"];
}
