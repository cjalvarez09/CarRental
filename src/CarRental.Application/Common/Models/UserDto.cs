using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Models;

public record UserDto(int Id, string Username, string Role);

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) => new(user.Id, user.Username, user.Role.ToString());
}
