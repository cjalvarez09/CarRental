using CarRental.Application.Rentals.Commands.CancelRental;
using CarRental.Application.Rentals.Commands.ModifyRental;
using CarRental.Application.Rentals.Commands.RegisterRental;

namespace CarRental.UnitTests.Application.Rentals;

public class RentalCommandValidatorTests
{
    private static readonly DateTime Tomorrow = DateTime.UtcNow.Date.AddDays(1);

    public class RegisterRental
    {
        private readonly RegisterRentalCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new RegisterRentalCommand(1, 1, Tomorrow, Tomorrow.AddDays(3));

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Given_ARentalStartingToday_When_Validating_Then_IsValid()
        {
            // Given
            var command = new RegisterRentalCommand(1, 1, DateTime.UtcNow.Date, Tomorrow);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Given_AStartDateInThePast_When_Validating_Then_Fails()
        {
            // Given
            var command = new RegisterRentalCommand(1, 1, DateTime.UtcNow.Date.AddDays(-1), Tomorrow);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains("Rental start date cannot be in the past.", result.Errors.Select(error => error.ErrorMessage));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Given_AnEndDateNotAfterTheStartDate_When_Validating_Then_Fails(int daysAfterStart)
        {
            // Given
            var command = new RegisterRentalCommand(1, 1, Tomorrow, Tomorrow.AddDays(daysAfterStart));

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains("Rental end date must be after the start date.", result.Errors.Select(error => error.ErrorMessage));
        }

        [Theory]
        [InlineData(0, 1, nameof(RegisterRentalCommand.CustomerId))]
        [InlineData(1, 0, nameof(RegisterRentalCommand.CarId))]
        public void Given_NonPositiveIds_When_Validating_Then_FailsOnTheOffendingId(
            int customerId, int carId, string expectedProperty)
        {
            // Given
            var command = new RegisterRentalCommand(customerId, carId, Tomorrow, Tomorrow.AddDays(2));

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
        }
    }

    public class ModifyRental
    {
        private readonly ModifyRentalCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new ModifyRentalCommand(1, Tomorrow, Tomorrow.AddDays(3));

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Given_AStartDateInThePast_When_Validating_Then_Fails()
        {
            // Given
            var command = new ModifyRentalCommand(1, DateTime.UtcNow.Date.AddDays(-1), Tomorrow);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Given_AnEndDateBeforeTheStartDate_When_Validating_Then_Fails()
        {
            // Given
            var command = new ModifyRentalCommand(1, Tomorrow, Tomorrow.AddDays(-1));

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Given_ANonPositiveRentalId_When_Validating_Then_FailsOnRentalId()
        {
            // Given
            var command = new ModifyRentalCommand(0, Tomorrow, Tomorrow.AddDays(2));

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(nameof(ModifyRentalCommand.RentalId), result.Errors.Select(error => error.PropertyName));
        }
    }

    public class CancelRental
    {
        private readonly CancelRentalCommandValidator _validator = new();

        [Fact]
        public void Given_APositiveId_When_Validating_Then_IsValid()
        {
            // Given
            var command = new CancelRentalCommand(1);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Given_ANonPositiveId_When_Validating_Then_Fails()
        {
            // Given
            var command = new CancelRentalCommand(0);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.False(result.IsValid);
        }
    }
}
