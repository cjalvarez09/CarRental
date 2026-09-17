using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new UsernameAlreadyExistsException(request.Username);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
