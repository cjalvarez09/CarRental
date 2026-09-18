using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Customers.Commands.DeleteCustomer;
using CarRental.Application.Customers.Commands.RegisterCustomer;
using CarRental.Application.Customers.Commands.UpdateCustomer;
using CarRental.Application.Customers.Queries.GetCustomerById;
using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Customers;

public class CustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IRentalRepository _rentalRepository = Substitute.For<IRentalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static Customer ExistingCustomer() =>
        new() { Id = 1, FullName = "Juan Perez", Address = "Calle 1", Email = "juan@example.com" };

    public class Register : CustomerCommandHandlerTests
    {
        [Fact]
        public async Task Given_ANewCustomer_When_Registering_Then_AddsItWithoutAnyLinkedUserAndSaves()
        {
            // Given
            var handler = new RegisterCustomerCommandHandler(_customerRepository, _unitOfWork);

            // When
            var result = await handler.Handle(
                new RegisterCustomerCommand("Ana Gomez", "Calle 2", "ana@example.com"), CancellationToken.None);

            // Then
            await _customerRepository.Received(1).AddAsync(
                Arg.Is<Customer>(customer =>
                    customer.FullName == "Ana Gomez" && customer.Address == "Calle 2" && customer.Email == "ana@example.com"),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            Assert.Equal("Ana Gomez", result.FullName);
        }
    }

    public class Update : CustomerCommandHandlerTests
    {
        [Fact]
        public async Task Given_AnUnknownCustomer_When_Updating_Then_ThrowsNotFoundException()
        {
            // Given
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Customer?)null);
            var handler = new UpdateCustomerCommandHandler(_customerRepository, _unitOfWork);

            // When
            Func<Task> act = () => handler.Handle(
                new UpdateCustomerCommand(1, "Ana", "Calle 2", "ana@example.com"), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task Given_AnExistingCustomer_When_Updating_Then_UpdatesItAndSaves()
        {
            // Given
            var customer = ExistingCustomer();
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(customer);
            var handler = new UpdateCustomerCommandHandler(_customerRepository, _unitOfWork);

            // When
            var result = await handler.Handle(
                new UpdateCustomerCommand(1, "Juan Perez Garcia", "Calle 9", "nuevo@example.com"), CancellationToken.None);

            // Then
            Assert.Equal("Juan Perez Garcia", customer.FullName);
            Assert.Equal("Calle 9", customer.Address);
            Assert.Equal("nuevo@example.com", customer.Email);
            Assert.Equal("nuevo@example.com", result.Email);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    public class Delete : CustomerCommandHandlerTests
    {
        private DeleteCustomerCommandHandler CreateHandler() => new(_customerRepository, _rentalRepository, _unitOfWork);

        [Fact]
        public async Task Given_AnUnknownCustomer_When_Deleting_Then_ThrowsNotFoundException()
        {
            // Given
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Customer?)null);

            // When
            Func<Task> act = () => CreateHandler().Handle(new DeleteCustomerCommand(1), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task Given_ACustomerWithRentals_When_Deleting_Then_ThrowsCustomerInUseExceptionAndDoesNotDelete()
        {
            // Given
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ExistingCustomer());
            _rentalRepository.ExistsForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(true);

            // When
            Func<Task> act = () => CreateHandler().Handle(new DeleteCustomerCommand(1), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<CustomerInUseException>(act);
            _customerRepository.DidNotReceive().Remove(Arg.Any<Customer>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Given_ACustomerWithoutRentals_When_Deleting_Then_RemovesItAndSaves()
        {
            // Given
            var customer = ExistingCustomer();
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(customer);
            _rentalRepository.ExistsForCustomerAsync(1, Arg.Any<CancellationToken>()).Returns(false);

            // When
            await CreateHandler().Handle(new DeleteCustomerCommand(1), CancellationToken.None);

            // Then
            _customerRepository.Received(1).Remove(customer);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    public class Queries : CustomerCommandHandlerTests
    {
        [Fact]
        public async Task Given_AnUnknownCustomer_When_GettingById_Then_ThrowsNotFoundException()
        {
            // Given
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Customer?)null);
            var handler = new GetCustomerByIdQueryHandler(_customerRepository);

            // When
            Func<Task> act = () => handler.Handle(new GetCustomerByIdQuery(1), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task Given_AnExistingCustomer_When_GettingById_Then_ReturnsItsDto()
        {
            // Given
            _customerRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ExistingCustomer());
            var handler = new GetCustomerByIdQueryHandler(_customerRepository);

            // When
            var result = await handler.Handle(new GetCustomerByIdQuery(1), CancellationToken.None);

            // Then
            Assert.Equal("Juan Perez", result.FullName);
        }
    }
}
