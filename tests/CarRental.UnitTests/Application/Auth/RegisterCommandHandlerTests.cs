using CarRental.Application.Auth.Commands.Register;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        _handler = new RegisterCommandHandler(_userRepository, _customerRepository, _passwordHasher, _unitOfWork);
    }

    [Fact]
    public async Task Given_UsernameAlreadyTaken_When_Handling_Then_ThrowsUsernameAlreadyExistsException()
    {
        // Given
        _userRepository.ExistsByUsernameAsync("taken", Arg.Any<CancellationToken>()).Returns(true);
        var command = new RegisterCommand("taken", "Secret123", "Employee", null, null, null);

        // When
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<UsernameAlreadyExistsException>(act);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_CustomerRole_When_Handling_Then_CreatesCustomerProfileLinkedToTheUser()
    {
        // Given
        _customerRepository
            .When(repository => repository.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()))
            .Do(call => call.Arg<Customer>().Id = 42);
        var command = new RegisterCommand("juan", "Secret123", "Customer", "Juan Perez", "Calle 1", "juan@example.com");

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        await _customerRepository.Received(1).AddAsync(
            Arg.Is<Customer>(customer =>
                customer.FullName == "Juan Perez" && customer.Address == "Calle 1" && customer.Email == "juan@example.com"),
            Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(user =>
                user.Username == "juan" &&
                user.Role == UserRole.Customer &&
                user.CustomerId == 42 &&
                user.PasswordHash == "hashed-password"),
            Arg.Any<CancellationToken>());
        Assert.Equal("Customer", result.Role);
        Assert.Equal("juan", result.Username);
    }

    [Fact]
    public async Task Given_EmployeeRole_When_Handling_Then_DoesNotCreateACustomerProfile()
    {
        // Given
        var command = new RegisterCommand("ana", "Secret123", "Employee", null, null, null);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        await _customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(user => user.Role == UserRole.Employee && user.CustomerId == null),
            Arg.Any<CancellationToken>());
        Assert.Equal("Employee", result.Role);
    }

    [Fact]
    public async Task Given_RoleInLowerCase_When_Handling_Then_ParsesItCaseInsensitively()
    {
        // Given
        var command = new RegisterCommand("ana", "Secret123", "employee", null, null, null);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.Equal("Employee", result.Role);
    }

    [Fact]
    public async Task Given_APlainPassword_When_Handling_Then_StoresOnlyTheHash()
    {
        // Given
        var command = new RegisterCommand("ana", "Secret123", "Employee", null, null, null);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        _passwordHasher.Received(1).Hash("Secret123");
        await _userRepository.DidNotReceive().AddAsync(
            Arg.Is<User>(user => user.PasswordHash == "Secret123"),
            Arg.Any<CancellationToken>());
    }
}
