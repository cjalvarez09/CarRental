using System.Security.Claims;
using CarRental.API.Security;
using Microsoft.AspNetCore.Http;

namespace CarRental.UnitTests.API;

public class CurrentUserServiceTests
{
    private static CurrentUserService ServiceFor(params Claim[] claims)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "test"))
        };

        return new CurrentUserService(new HttpContextAccessor { HttpContext = context });
    }

    [Fact]
    public void Given_AnAuthenticatedPrincipal_When_ReadingProperties_Then_ReturnsTheClaimValues()
    {
        // Given
        var service = ServiceFor(
            new Claim(ClaimTypes.NameIdentifier, "7"),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("customerId", "42"));

        // When
        var (userId, role, customerId) = (service.UserId, service.Role, service.CustomerId);

        // Then
        Assert.Equal(7, userId);
        Assert.Equal("Customer", role);
        Assert.Equal(42, customerId);
    }

    [Fact]
    public void Given_AnEmployeeWithoutTheCustomerIdClaim_When_ReadingCustomerId_Then_IsNull()
    {
        // Given
        var service = ServiceFor(
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Employee"));

        // When
        var customerId = service.CustomerId;

        // Then
        Assert.Null(customerId);
    }

    [Fact]
    public void Given_NonNumericIdClaims_When_ReadingProperties_Then_TheIdsAreNull()
    {
        // Given
        var service = ServiceFor(
            new Claim(ClaimTypes.NameIdentifier, "not-a-number"),
            new Claim("customerId", "abc"));

        // When
        var (userId, customerId) = (service.UserId, service.CustomerId);

        // Then
        Assert.Null(userId);
        Assert.Null(customerId);
    }

    [Fact]
    public void Given_NoHttpContext_When_ReadingProperties_Then_AreAllNull()
    {
        // Given
        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = null });

        // When
        var (userId, role, customerId) = (service.UserId, service.Role, service.CustomerId);

        // Then
        Assert.Null(userId);
        Assert.Null(role);
        Assert.Null(customerId);
    }
}
