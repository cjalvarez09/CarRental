using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Rentals.Commands.ModifyRental;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Rentals;

public class ModifyRentalCommandHandlerTests
{
    private static readonly DateTime NewStart = new(2030, 11, 1);
    private static readonly DateTime NewEnd = new(2030, 11, 8);

    private readonly IRentalRepository _rentalRepository = Substitute.For<IRentalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ModifyRentalCommandHandler _handler;

    public ModifyRentalCommandHandlerTests()
    {
        _handler = new ModifyRentalCommandHandler(_rentalRepository, _unitOfWork);
    }

    private static Rental ActiveRental() => new()
    {
        Id = 5,
        CarId = 3,
        Car = new Car { Id = 3, Type = "Sedan", Model = "Corolla" },
        CustomerId = 1,
        Customer = new Customer { Id = 1, FullName = "Juan Perez", Address = "Calle 1", Email = "juan@example.com" },
        StartDate = new DateTime(2030, 10, 1),
        EndDate = new DateTime(2030, 10, 5),
        Status = RentalStatus.Active
    };

    [Fact]
    public async Task Given_AnUnknownRental_When_Modifying_Then_ThrowsNotFoundException()
    {
        // Given
        _rentalRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((Rental?)null);

        // When
        Func<Task> act = () => _handler.Handle(new ModifyRentalCommand(5, NewStart, NewEnd), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Given_ACancelledRental_When_Modifying_Then_ThrowsDomainException()
    {
        // Given
        var rental = ActiveRental();
        rental.Status = RentalStatus.Cancelled;
        _rentalRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(rental);

        // When
        Func<Task> act = () => _handler.Handle(new ModifyRentalCommand(5, NewStart, NewEnd), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<DomainException>(act);
    }

    [Fact]
    public async Task Given_NewDatesOverlappingAnotherRental_When_Modifying_Then_ThrowsCarNotAvailableException()
    {
        // Given
        var rental = ActiveRental();
        _rentalRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(rental);
        _rentalRepository.HasOverlapAsync(3, NewStart, NewEnd, 5, Arg.Any<CancellationToken>()).Returns(true);

        // When
        Func<Task> act = () => _handler.Handle(new ModifyRentalCommand(5, NewStart, NewEnd), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<CarNotAvailableException>(act);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_FreeDates_When_Modifying_Then_UpdatesTheRentalAndSaves()
    {
        // Given
        var rental = ActiveRental();
        _rentalRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(rental);

        // When
        var result = await _handler.Handle(new ModifyRentalCommand(5, NewStart, NewEnd), CancellationToken.None);

        // Then
        Assert.Equal(NewStart, rental.StartDate);
        Assert.Equal(NewEnd, rental.EndDate);
        Assert.Equal(NewStart, result.StartDate);
        Assert.Equal(NewEnd, result.EndDate);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AnActiveRental_When_Modifying_Then_ChecksOverlapExcludingTheRentalItself()
    {
        // Given
        var rental = ActiveRental();
        _rentalRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(rental);

        // When
        await _handler.Handle(new ModifyRentalCommand(5, NewStart, NewEnd), CancellationToken.None);

        // Then
        await _rentalRepository.Received(1).HasOverlapAsync(3, NewStart, NewEnd, 5, Arg.Any<CancellationToken>());
    }
}
