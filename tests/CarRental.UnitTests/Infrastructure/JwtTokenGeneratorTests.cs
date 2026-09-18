using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace CarRental.UnitTests.Infrastructure;

public class JwtTokenGeneratorTests
{
    private const string Secret = "unit-test-secret-that-is-long-enough-for-hmac-sha256";

    private static JwtTokenGenerator CreateGenerator(Dictionary<string, string?>? overrides = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = Secret,
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpirationMinutes"] = "30"
        };

        foreach (var (key, value) in overrides ?? [])
            settings[key] = value;

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new JwtTokenGenerator(configuration);
    }

    private static JwtSecurityToken Read(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    private static string ClaimValue(JwtSecurityToken token, string type) =>
        token.Claims.Single(claim => claim.Type == type).Value;

    [Fact]
    public void Given_ACustomerUser_When_GeneratingAToken_Then_IncludesIdentityRoleAndCustomerIdClaims()
    {
        // Given
        var user = new User { Id = 7, Username = "juan", Role = UserRole.Customer, CustomerId = 42 };

        // When
        var (token, _) = CreateGenerator().Generate(user);

        // Then
        var jwt = Read(token);
        Assert.Equal("7", ClaimValue(jwt, JwtRegisteredClaimNames.Sub));
        Assert.Equal("7", ClaimValue(jwt, ClaimTypes.NameIdentifier));
        Assert.Equal("juan", ClaimValue(jwt, ClaimTypes.Name));
        Assert.Equal("Customer", ClaimValue(jwt, ClaimTypes.Role));
        Assert.Equal("42", ClaimValue(jwt, "customerId"));
    }

    [Fact]
    public void Given_AnEmployeeUser_When_GeneratingAToken_Then_HasNoCustomerIdClaim()
    {
        // Given
        var user = new User { Id = 1, Username = "ana", Role = UserRole.Employee, CustomerId = null };

        // When
        var (token, _) = CreateGenerator().Generate(user);

        // Then
        var jwt = Read(token);
        Assert.Equal("Employee", ClaimValue(jwt, ClaimTypes.Role));
        Assert.DoesNotContain(jwt.Claims, claim => claim.Type == "customerId");
    }

    [Fact]
    public void Given_AConfiguredExpiration_When_GeneratingAToken_Then_SetsIssuerAudienceAndExpiration()
    {
        // Given
        var before = DateTime.UtcNow;

        // When
        var (token, expiresAtUtc) = CreateGenerator().Generate(new User { Id = 1, Username = "ana" });

        // Then
        var jwt = Read(token);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.InRange(expiresAtUtc, before.AddMinutes(30), DateTime.UtcNow.AddMinutes(30));
        Assert.Equal(expiresAtUtc, jwt.ValidTo, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Given_NoConfiguredExpiration_When_GeneratingAToken_Then_DefaultsToSixtyMinutes()
    {
        // Given
        var generator = CreateGenerator(new Dictionary<string, string?> { ["Jwt:ExpirationMinutes"] = null });
        var before = DateTime.UtcNow;

        // When
        var (_, expiresAtUtc) = generator.Generate(new User { Id = 1, Username = "ana" });

        // Then
        Assert.InRange(expiresAtUtc, before.AddMinutes(60), DateTime.UtcNow.AddMinutes(60));
    }

    [Fact]
    public void Given_NoConfiguredSecret_When_GeneratingAToken_Then_ThrowsInvalidOperationException()
    {
        // Given
        var generator = new JwtTokenGenerator(new ConfigurationBuilder().Build());

        // When
        Action act = () => generator.Generate(new User { Id = 1, Username = "ana" });

        // Then
        Assert.Throws<InvalidOperationException>(act);
    }
}
