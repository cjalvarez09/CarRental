namespace CarRental.IntegrationTests.Infrastructure;

public sealed record TestUser(HttpClient Client, string Username, string Email);
