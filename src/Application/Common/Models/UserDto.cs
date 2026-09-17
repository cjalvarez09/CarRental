using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Models;

public record UserDto(int Id, string Username);

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) => new(user.Id, user.Username);
}
