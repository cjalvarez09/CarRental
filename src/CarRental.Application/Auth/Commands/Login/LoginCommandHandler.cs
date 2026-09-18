using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new InvalidCredentialsException();

        var (token, expiresAtUtc) = jwtTokenGenerator.Generate(user);

        return new AuthResultDto(token, expiresAtUtc, user.ToDto());
    }
}
