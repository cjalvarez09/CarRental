namespace CarRental.Application.Common.Models;

public record AuthResultDto(string Token, DateTime ExpiresAtUtc, UserDto User);
