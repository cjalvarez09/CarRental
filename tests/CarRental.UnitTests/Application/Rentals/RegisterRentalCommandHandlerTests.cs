using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Rentals.Commands.RegisterRental;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Rentals;

public class RegisterRentalCommandHandlerTests
{
    private static readonly DateTime Start = new(2030, 10, 1);
    private static readonly DateTime End = new(2030, 10, 5);

    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly ICarRepository _carRepository = Substitute.For<ICarRepository>();
    private readonly IRentalRepository _rentalRepository = Substitute.For<IRentalRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterRentalCommandHandler _handler;

    public RegisterRentalCommandHandlerTests()
    {
        _currentUser.Role.Returns(Roles.Employee);
        _handler = new RegisterRentalCommandHandler(
            _customerRepository, _carRepository, _rentalRepository, _currentUser, _unitOfWork);
    }

    private void GivenCustomer(int id, string fullName = "Juan Perez") =>
        _customerRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new Customer { Id = id, FullName = fullName, Address = "Calle 1", Email = "juan@example.com" });

    private void GivenCar(int id) =>
        _carRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new Car { Id = id, Type = "Sedan", Model = "Corolla" });

    [Fact]
    public async Task Given_AnUnknownCustomer_When_Booking_Then_ThrowsNotFoundException()
    {
        // Given
        GivenCar(1);
        _customerRepository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        // When
        Func<Task> act = () => _handler.Handle(new RegisterRentalCommand(99, 1, Start, End), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Given_AnUnknownCar_When_Booking_Then_ThrowsNotFoundException()
    {
        // Given
        GivenCustomer(1);
        _carRepository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Car?)null);

        // When
        Func<Task> act = () => _handler.Handle(new RegisterRentalCommand(1, 99, Start, End), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Given_ACarAlreadyRentedForThoseDates_When_Booking_Then_ThrowsCarNotAvailableException()
    {
        // Given
        GivenCustomer(1);
        GivenCar(1);
        _rentalRepository
            .HasOverlapAsync(1, Start, End, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // When
        Func<Task> act = () => _handler.Handle(new RegisterRentalCommand(1, 1, Start, End), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<CarNotAvailableException>(act);
        await _rentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AValidBooking_When_Booking_Then_CreatesAnActiveRentalAndSavesIt()
    {
        // Given
        GivenCustomer(1);
        GivenCar(2);

        // When
        var result = await _handler.Handle(new RegisterRentalCommand(1, 2, Start, End), CancellationToken.None);

        // Then
        await _rentalRepository.Received(1).AddAsync(
            Arg.Is<Rental>(rental =>
                rental.CustomerId == 1 &&
                rental.CarId == 2 &&
                rental.StartDate == Start &&
                rental.EndDate == End &&
                rental.Status == RentalStatus.Active),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("Active", result.Status);
        Assert.Equal(1, result.Customer.Id);
        Assert.Equal(2, result.Car.Id);
    }

    [Fact]
    public async Task Given_AnEmployeeCaller_When_Booking_Then_BooksForTheRequestedCustomer()
    {
        // Given
        _currentUser.Role.Returns(Roles.Employee);
        GivenCustomer(2, "Otra Persona");
        GivenCar(1);

        // When
        var result = await _handler.Handle(new RegisterRentalCommand(2, 1, Start, End), CancellationToken.None);

        // Then
        Assert.Equal(2, result.Customer.Id);
        Assert.Equal("Otra Persona", result.Customer.FullName);
    }

    [Fact]
    public async Task Given_ACustomerCaller_When_BookingForAnotherCustomer_Then_BooksForThemselvesInstead()
    {
        // Given
        _currentUser.Role.Returns(Roles.Customer);
        _currentUser.CustomerId.Returns(1);
        GivenCustomer(1, "Cliente Uno");
        GivenCustomer(2, "Otra Persona");
        GivenCar(1);

        // When
        var result = await _handler.Handle(new RegisterRentalCommand(2, 1, Start, End), CancellationToken.None);

        // Then
        Assert.Equal(1, result.Customer.Id);
        Assert.Equal("Cliente Uno", result.Customer.FullName);
        await _customerRepository.DidNotReceive().GetByIdAsync(2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ACustomerCallerWithoutALinkedProfile_When_Booking_Then_ThrowsDomainException()
    {
        // Given
        _currentUser.Role.Returns(Roles.Customer);
        _currentUser.CustomerId.Returns((int?)null);

        // When
        Func<Task> act = () => _handler.Handle(new RegisterRentalCommand(1, 1, Start, End), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<DomainException>(act);
        await _rentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>(), Arg.Any<CancellationToken>());
    }
}
