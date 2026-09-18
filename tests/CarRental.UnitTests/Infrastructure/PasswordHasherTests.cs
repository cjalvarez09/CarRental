using CarRental.Infrastructure.Security;

namespace CarRental.UnitTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Given_APassword_When_Hashing_Then_DoesNotReturnThePlainPassword()
    {
        // Given
        const string password = "Secret123";

        // When
        var hash = _hasher.Hash(password);

        // Then
        Assert.NotEqual(password, hash);
        Assert.DoesNotContain(password, hash);
    }

    [Fact]
    public void Given_TheSamePassword_When_HashingTwice_Then_ProducesDifferentHashesBecauseOfTheSalt()
    {
        // Given
        const string password = "Secret123";

        // When
        var first = _hasher.Hash(password);
        var second = _hasher.Hash(password);

        // Then
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Given_TheCorrectPassword_When_Verifying_Then_ReturnsTrue()
    {
        // Given
        var hash = _hasher.Hash("Secret123");

        // When
        var result = _hasher.Verify(hash, "Secret123");

        // Then
        Assert.True(result);
    }

    [Fact]
    public void Given_AWrongPassword_When_Verifying_Then_ReturnsFalse()
    {
        // Given
        var hash = _hasher.Hash("Secret123");

        // When
        var result = _hasher.Verify(hash, "secret123");

        // Then
        Assert.False(result);
    }
}
