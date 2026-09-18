using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Mappings;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) => new(user.Id, user.Username, user.Role.ToString());
}
