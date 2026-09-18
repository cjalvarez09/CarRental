using CarRental.Application.Auth.Commands.Login;
using CarRental.Application.Cars.Commands.CreateCar;
using CarRental.Application.Cars.Commands.DeleteCar;
using CarRental.Application.Cars.Commands.UpdateCar;
using CarRental.Application.Customers.Commands.DeleteCustomer;
using CarRental.Application.Customers.Commands.RegisterCustomer;
using CarRental.Application.Customers.Commands.UpdateCustomer;

namespace CarRental.UnitTests.Application;

public class CommandValidatorTests
{
    public class CreateCar
    {
        private readonly CreateCarCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new CreateCarCommand("Sedan", "Corolla");

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("", "Corolla", nameof(CreateCarCommand.Type))]
        [InlineData("Sedan", "", nameof(CreateCarCommand.Model))]
        public void Given_AnEmptyField_When_Validating_Then_FailsOnThatField(string type, string model, string expectedProperty)
        {
            // Given
            var command = new CreateCarCommand(type, model);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
        }
    }

    public class UpdateCar
    {
        private readonly UpdateCarCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new UpdateCarCommand(1, "Sedan", "Corolla");

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0, "Sedan", "Corolla", nameof(UpdateCarCommand.CarId))]
        [InlineData(1, "", "Corolla", nameof(UpdateCarCommand.Type))]
        [InlineData(1, "Sedan", "", nameof(UpdateCarCommand.Model))]
        public void Given_AnInvalidField_When_Validating_Then_FailsOnThatField(
            int carId, string type, string model, string expectedProperty)
        {
            // Given
            var command = new UpdateCarCommand(carId, type, model);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
        }
    }

    public class DeleteCar
    {
        private readonly DeleteCarCommandValidator _validator = new();

        [Fact]
        public void Given_APositiveId_When_Validating_Then_IsValid()
        {
            // Given
            var command = new DeleteCarCommand(1);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Given_ANonPositiveId_When_Validating_Then_Fails()
        {
            // Given
            var command = new DeleteCarCommand(0);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.False(result.IsValid);
        }
    }

    public class RegisterCustomer
    {
        private readonly RegisterCustomerCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new RegisterCustomerCommand("Ana Gomez", "Calle 2", "ana@example.com");

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("", "Calle 2", "ana@example.com", nameof(RegisterCustomerCommand.FullName))]
        [InlineData("Ana", "", "ana@example.com", nameof(RegisterCustomerCommand.Address))]
        [InlineData("Ana", "Calle 2", "", nameof(RegisterCustomerCommand.Email))]
        [InlineData("Ana", "Calle 2", "not-an-email", nameof(RegisterCustomerCommand.Email))]
        public void Given_AnInvalidField_When_Validating_Then_FailsOnThatField(
            string fullName, string address, string email, string expectedProperty)
        {
            // Given
            var command = new RegisterCustomerCommand(fullName, address, email);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
        }
    }

    public class UpdateCustomer
    {
        private readonly UpdateCustomerCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new UpdateCustomerCommand(1, "Ana", "Calle 2", "ana@example.com");

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0, "Ana", "Calle 2", "ana@example.com", nameof(UpdateCustomerCommand.CustomerId))]
        [InlineData(1, "", "Calle 2", "ana@example.com", nameof(UpdateCustomerCommand.FullName))]
        [InlineData(1, "Ana", "", "ana@example.com", nameof(UpdateCustomerCommand.Address))]
        [InlineData(1, "Ana", "Calle 2", "not-an-email", nameof(UpdateCustomerCommand.Email))]
        public void Given_AnInvalidField_When_Validating_Then_FailsOnThatField(
            int id, string fullName, string address, string email, string expectedProperty)
        {
            // Given
            var command = new UpdateCustomerCommand(id, fullName, address, email);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
        }
    }

    public class DeleteCustomer
    {
        private readonly DeleteCustomerCommandValidator _validator = new();

        [Fact]
        public void Given_APositiveId_When_Validating_Then_IsValid()
        {
            // Given
            var command = new DeleteCustomerCommand(1);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Given_ANonPositiveId_When_Validating_Then_Fails()
        {
            // Given
            var command = new DeleteCustomerCommand(0);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.False(result.IsValid);
        }
    }

    public class Login
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Given_AValidCommand_When_Validating_Then_IsValid()
        {
            // Given
            var command = new LoginCommand("juan", "Secret123");

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("", "Secret123", nameof(LoginCommand.Username))]
        [InlineData("juan", "", nameof(LoginCommand.Password))]
        public void Given_AnEmptyField_When_Validating_Then_FailsOnThatField(string username, string password, string expectedProperty)
        {
            // Given
            var command = new LoginCommand(username, password);

            // When
            var result = _validator.Validate(command);

            // Then
            Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
        }
    }
}
