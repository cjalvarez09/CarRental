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
    private static readonly DateTime Today = DateTime.UtcNow.Date;

    private readonly IRentalRepository _rentalRepository = Substitute.For<IRentalRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelRentalCommandHandler _handler;

    public CancelRentalCommandHandlerTests()
    {
        _currentUser.Role.Returns(Roles.Employee);
        _handler = new CancelRentalCommandHandler(_rentalRepository, _currentUser, _unitOfWork);
    }

    private Rental GivenRental(int startsInDays, int lastsDays = 3, int customerId = 1, RentalStatus status = RentalStatus.Active)
    {
        var rental = new Rental
        {
            Id = 1,
            CustomerId = customerId,
            StartDate = Today.AddDays(startsInDays),
            EndDate = Today.AddDays(startsInDays + lastsDays),
            Status = status
        };
        _rentalRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(rental);
        return rental;
    }

    private void GivenCallerIsCustomer(int customerId)
    {
        _currentUser.Role.Returns(Roles.Customer);
        _currentUser.CustomerId.Returns(customerId);
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
        GivenRental(startsInDays: 5, status: RentalStatus.Cancelled);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<DomainException>(act);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AnActiveRental_When_AnEmployeeCancelsIt_Then_MarksItCancelledAndSaves()
    {
        // Given
        var rental = GivenRental(startsInDays: 5);

        // When
        await _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        Assert.Equal(RentalStatus.Cancelled, rental.Status);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ARentalInProgress_When_AnEmployeeCancelsIt_Then_ItIsCancelled()
    {
        // Given
        var rental = GivenRental(startsInDays: -1);

        // When
        await _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        Assert.Equal(RentalStatus.Cancelled, rental.Status);
    }

    [Fact]
    public async Task Given_TheCustomersOwnUpcomingRental_When_TheCustomerCancelsIt_Then_MarksItCancelledAndSaves()
    {
        // Given
        GivenCallerIsCustomer(customerId: 1);
        var rental = GivenRental(startsInDays: 1, customerId: 1);

        // When
        await _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        Assert.Equal(RentalStatus.Cancelled, rental.Status);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task Given_ARentalThatAlreadyStarted_When_TheCustomerCancelsIt_Then_ThrowsRentalInProgressException(int startsInDays)
    {
        // Given
        GivenCallerIsCustomer(customerId: 1);
        var rental = GivenRental(startsInDays, customerId: 1);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<RentalInProgressException>(act);
        Assert.Equal(RentalStatus.Active, rental.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ANotStartedRentalOfAnotherCustomer_When_TheCustomerCancelsIt_Then_ThrowsNotFoundException()
    {
        // Given
        GivenCallerIsCustomer(customerId: 1);
        var rental = GivenRental(startsInDays: 5, customerId: 2);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(RentalStatus.Active, rental.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ACustomerWithoutALinkedProfile_When_CancellingAnyRental_Then_ThrowsNotFoundException()
    {
        // Given
        _currentUser.Role.Returns(Roles.Customer);
        _currentUser.CustomerId.Returns((int?)null);
        GivenRental(startsInDays: 5, customerId: 1);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Given_TheCustomersOwnCancelledRental_When_TheCustomerCancelsItAgain_Then_ThrowsDomainException()
    {
        // Given
        GivenCallerIsCustomer(customerId: 1);
        GivenRental(startsInDays: 5, customerId: 1, status: RentalStatus.Cancelled);

        // When
        Func<Task> act = () => _handler.Handle(new CancelRentalCommand(1), CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(act);
        Assert.IsNotType<RentalInProgressException>(exception);
    }
}
