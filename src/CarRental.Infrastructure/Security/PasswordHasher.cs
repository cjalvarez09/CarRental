using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRental.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _identityHasher = new();

    public string Hash(string password) => _identityHasher.HashPassword(default!, password);

    public bool Verify(string hash, string password) =>
        _identityHasher.VerifyHashedPassword(default!, hash, password) != PasswordVerificationResult.Failed;
}
