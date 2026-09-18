using CarRental.Application.Common.Interfaces;
using CarRental.Application.Common.Mappings;
using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    ICustomerRepository customerRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new UsernameAlreadyExistsException(request.Username);

        var role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

        int? customerId = null;

        if (role == UserRole.Customer)
        {
            var customer = new Customer
            {
                FullName = request.FullName!,
                Address = request.Address!,
                Email = request.Email!
            };

            await customerRepository.AddAsync(customer, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            customerId = customer.Id;
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = role,
            CustomerId = customerId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
