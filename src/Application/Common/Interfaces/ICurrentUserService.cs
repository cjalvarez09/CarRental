namespace CarRental.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Role { get; }
    int? CustomerId { get; }
}
