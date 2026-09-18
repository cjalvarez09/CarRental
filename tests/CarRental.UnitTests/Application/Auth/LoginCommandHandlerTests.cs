using CarRental.Application.Auth.Commands.Login;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Given_AnUnknownUsername_When_LoggingIn_Then_ThrowsInvalidCredentialsException()
    {
        // Given
        _userRepository.GetByUsernameAsync("ghost", Arg.Any<CancellationToken>()).Returns((User?)null);

        // When
        Func<Task> act = () => _handler.Handle(new LoginCommand("ghost", "Secret123"), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<InvalidCredentialsException>(act);
        _jwtTokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }

    [Fact]
    public async Task Given_AWrongPassword_When_LoggingIn_Then_ThrowsInvalidCredentialsException()
    {
        // Given
        var user = new User { Id = 1, Username = "juan", PasswordHash = "hash" };
        _userRepository.GetByUsernameAsync("juan", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("hash", "wrong").Returns(false);

        // When
        Func<Task> act = () => _handler.Handle(new LoginCommand("juan", "wrong"), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<InvalidCredentialsException>(act);
        _jwtTokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }

    [Fact]
    public async Task Given_ValidCredentials_When_LoggingIn_Then_ReturnsTheTokenAndTheUser()
    {
        // Given
        var user = new User { Id = 7, Username = "juan", PasswordHash = "hash", Role = UserRole.Employee };
        var expiresAt = DateTime.UtcNow.AddHours(1);
        _userRepository.GetByUsernameAsync("juan", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("hash", "Secret123").Returns(true);
        _jwtTokenGenerator.Generate(user).Returns(("jwt-token", expiresAt));

        // When
        var result = await _handler.Handle(new LoginCommand("juan", "Secret123"), CancellationToken.None);

        // Then
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal(expiresAt, result.ExpiresAtUtc);
        Assert.Equal(7, result.User.Id);
        Assert.Equal("juan", result.User.Username);
        Assert.Equal("Employee", result.User.Role);
    }
}
