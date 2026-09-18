using System.Net;
using System.Net.Http.Json;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests;

public class CorsTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    private const string AngularOrigin = "http://localhost:4200";
    private const string OtherOrigin = "http://evil.example.com";

    private static HttpRequestMessage Preflight(string origin, string path = "/api/cars")
    {
        var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        return request;
    }

    private static string? AllowedOriginOf(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values) ? values.Single() : null;

    [Fact]
    public async Task Given_TheAngularOrigin_When_SendingAPreflightRequest_Then_ItIsAllowedWithTheHeadersItAsksFor()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.SendAsync(Preflight(AngularOrigin));

        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AngularOrigin, AllowedOriginOf(response));
        var allowedHeaders = string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers"));
        Assert.Contains("authorization", allowedHeaders, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("POST", response.Headers.GetValues("Access-Control-Allow-Methods").Single());
    }

    [Fact]
    public async Task Given_AnUnknownOrigin_When_SendingAPreflightRequest_Then_ItIsNotAllowed()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.SendAsync(Preflight(OtherOrigin));

        // Then
        Assert.Null(AllowedOriginOf(response));
    }

    [Fact]
    public async Task Given_TheAngularOrigin_When_TheApiAnswersAnUnauthenticatedRequest_Then_TheResponseStillAllowsTheOrigin()
    {
        // Given
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/cars");
        request.Headers.Add("Origin", AngularOrigin);

        // When
        var response = await client.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(AngularOrigin, AllowedOriginOf(response));
    }

    [Fact]
    public async Task Given_TheAngularOrigin_When_TheGlobalExceptionHandlerBuildsTheError_Then_TheResponseStillAllowsTheOrigin()
    {
        // Given
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new { username = "x", password = "y", role = "SuperAdmin" })
        };
        request.Headers.Add("Origin", AngularOrigin);

        // When
        var response = await client.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(AngularOrigin, AllowedOriginOf(response));
    }

    [Fact]
    public async Task Given_AnUnknownOrigin_When_CallingTheApi_Then_TheResponseDoesNotAllowIt()
    {
        // Given
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/cars");
        request.Headers.Add("Origin", OtherOrigin);

        // When
        var response = await client.SendAsync(request);

        // Then
        Assert.Null(AllowedOriginOf(response));
    }
}
