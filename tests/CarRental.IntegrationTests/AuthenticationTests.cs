using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CarRental.Application.Common.Models;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests;

public class AuthenticationTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    private static string NewUsername() => $"user-{Guid.NewGuid():N}";

    [Fact]
    public async Task Given_ANewCustomer_When_RegisteringAndLoggingIn_Then_TheTokenGrantsAccessToProtectedEndpoints()
    {
        // Given
        var client = factory.CreateClient();
        var username = NewUsername();

        // When
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            password = TestApi.Password,
            role = "Customer",
            fullName = "Juan Perez",
            address = "Calle 1",
            email = $"{username}@example.com"
        });
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username, password = TestApi.Password });
        var auth = await login.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        var protectedCall = await client.GetAsync(
            $"/api/cars/availability?type=Sedan&startDate={TestApi.FutureDate(5):yyyy-MM-dd}&endDate={TestApi.FutureDate(7):yyyy-MM-dd}");

        // Then
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal("Customer", auth.User.Role);
        Assert.Equal(HttpStatusCode.OK, protectedCall.StatusCode);
    }

    [Fact]
    public async Task Given_NoToken_When_CallingAProtectedEndpoint_Then_Returns401WithAProblemDetailsBody()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.GetAsync("/api/cars");

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(401, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Given_AMalformedToken_When_CallingAProtectedEndpoint_Then_Returns401()
    {
        // Given
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        // When
        var response = await client.GetAsync("/api/cars");

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Given_ATakenUsername_When_RegisteringAgain_Then_Returns409()
    {
        // Given
        var client = factory.CreateClient();
        var username = NewUsername();
        var body = new { username, password = TestApi.Password, role = "Employee" };
        await client.PostAsJsonAsync("/api/auth/register", body);

        // When
        var response = await client.PostAsJsonAsync("/api/auth/register", body);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Given_AWrongPassword_When_LoggingIn_Then_Returns401()
    {
        // Given
        var client = factory.CreateClient();
        var username = NewUsername();
        await client.PostAsJsonAsync("/api/auth/register", new { username, password = TestApi.Password, role = "Employee" });

        // When
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password = "wrong-password" });

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Given_AnInvalidRole_When_RegisteringWithATextPlainAcceptHeader_Then_Returns400WithTheDetailedErrors()
    {
        // Given
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new { username = NewUsername(), password = TestApi.Password, role = "SuperAdmin" })
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        // When
        var response = await client.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("Role", out _));
    }

    [Fact]
    public async Task Given_ACustomerWithoutProfileFields_When_Registering_Then_Returns400ListingTheMissingFields()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new { username = NewUsername(), password = TestApi.Password, role = "Customer" });

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("FullName", out _));
        Assert.True(errors.TryGetProperty("Address", out _));
        Assert.True(errors.TryGetProperty("Email", out _));
    }
}
