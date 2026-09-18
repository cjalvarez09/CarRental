using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Application.Rentals.Commands.CancelRental;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Rentals;

public class CancelRentalCommandHandlerTests
{
    private readonly IRentalRepository _rentalRepository = Substitute.For<IRentalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelRentalCommandHandler _handler;

    public CancelRentalCommandHandlerTests()
    {
        _handler = new CancelRentalCommandHandler(_rentalRepository, _unitOfWork);
    }

    [Fact]
    public async Task Given_AnUnknownRental_When_Cancelling_Then_ThrowsNotFoundException()
    {
        // Given
        _rentalRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Rental?)null);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Given_AnAlreadyCancelledRental_When_Cancelling_Then_ThrowsDomainException()
    {
        // Given
        var rental = new Rental { Id = 1, Status = RentalStatus.Cancelled };
        _rentalRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(rental);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<DomainException>(act);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AnActiveRental_When_Cancelling_Then_MarksItCancelledAndSaves()
    {
        // Given
        var rental = new Rental { Id = 1, Status = RentalStatus.Active };
        _rentalRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(rental);

        // When
        await _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        Assert.Equal(RentalStatus.Cancelled, rental.Status);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
