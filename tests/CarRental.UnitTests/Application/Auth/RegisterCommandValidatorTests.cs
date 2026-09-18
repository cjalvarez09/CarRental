using CarRental.Application.Auth.Commands.Register;

namespace CarRental.UnitTests.Application.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Given_ACustomerWithAllProfileFields_When_Validating_Then_IsValid()
    {
        // Given
        var command = new RegisterCommand("juan", "Secret123", "Customer", "Juan Perez", "Calle 1", "juan@example.com");

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Given_AnEmployeeWithoutProfileFields_When_Validating_Then_IsValid()
    {
        // Given
        var command = new RegisterCommand("ana", "Secret123", "Employee", null, null, null);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Given_ACustomerWithoutProfileFields_When_Validating_Then_FailsForEachProfileField()
    {
        // Given
        var command = new RegisterCommand("juan", "Secret123", "Customer", null, null, null);

        // When
        var result = _validator.Validate(command);

        // Then
        var errors = result.Errors.Select(error => error.PropertyName).ToList();
        Assert.Contains(nameof(RegisterCommand.FullName), errors);
        Assert.Contains(nameof(RegisterCommand.Address), errors);
        Assert.Contains(nameof(RegisterCommand.Email), errors);
    }

    [Fact]
    public void Given_ACustomerWithAnInvalidEmail_When_Validating_Then_FailsOnEmail()
    {
        // Given
        var command = new RegisterCommand("juan", "Secret123", "Customer", "Juan Perez", "Calle 1", "not-an-email");

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.Contains(nameof(RegisterCommand.Email), result.Errors.Select(error => error.PropertyName));
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("")]
    public void Given_AnInvalidRole_When_Validating_Then_FailsOnRole(string role)
    {
        // Given
        var command = new RegisterCommand("juan", "Secret123", role, null, null, null);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.Contains(nameof(RegisterCommand.Role), result.Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public void Given_AnInvalidRole_When_Validating_Then_DoesNotAlsoDemandProfileFields()
    {
        // Given
        var command = new RegisterCommand("juan", "Secret123", "SuperAdmin", null, null, null);

        // When
        var result = _validator.Validate(command);

        // Then
        var errors = result.Errors.Select(error => error.PropertyName).ToList();
        Assert.Equal([nameof(RegisterCommand.Role)], errors);
    }

    [Theory]
    [InlineData("ab", "Secret123", nameof(RegisterCommand.Username))]
    [InlineData("", "Secret123", nameof(RegisterCommand.Username))]
    [InlineData("juan", "12345", nameof(RegisterCommand.Password))]
    [InlineData("juan", "", nameof(RegisterCommand.Password))]
    public void Given_ShortOrEmptyCredentials_When_Validating_Then_FailsOnTheOffendingProperty(
        string username, string password, string expectedProperty)
    {
        // Given
        var command = new RegisterCommand(username, password, "Employee", null, null, null);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.Contains(expectedProperty, result.Errors.Select(error => error.PropertyName));
    }
}
