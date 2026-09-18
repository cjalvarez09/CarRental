using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarRental.Application.Common.Models;

namespace CarRental.IntegrationTests.Infrastructure;

public static class TestApi
{
    public const string Password = "Secret123";

    public static Task<TestUser> RegisterEmployeeAsync(this CarRentalApiFactory factory) =>
        RegisterAsync(factory, "Employee");

    public static Task<TestUser> RegisterCustomerAsync(this CarRentalApiFactory factory) =>
        RegisterAsync(factory, "Customer");

    public static string UniqueCarType() => $"Type-{Guid.NewGuid():N}";

    public static DateTime FutureDate(int daysFromNow) => DateTime.UtcNow.Date.AddDays(daysFromNow);

    public static async Task<CarDto> CreateCarAsync(this HttpClient employee, string type, string model = "Corolla")
    {
        var response = await employee.PostAsJsonAsync("/api/cars", new { type, model });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CarDto>())!;
    }

    public static async Task<int> GetCustomerIdAsync(this HttpClient employee, string email)
    {
        var customers = await employee.GetFromJsonAsync<List<CustomerDto>>("/api/customers");
        return customers!.Single(customer => customer.Email == email).Id;
    }

    public static Task<HttpResponseMessage> BookAsync(
        this HttpClient client, int customerId, int carId, DateTime startDate, DateTime endDate) =>
        client.PostAsJsonAsync("/api/rentals", new { customerId, carId, startDate, endDate });

    public static async Task<List<int>> AvailableCarIdsAsync(
        this HttpClient client, string type, DateTime startDate, DateTime endDate)
    {
        var url = $"/api/cars/availability?type={type}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var cars = await client.GetFromJsonAsync<List<CarDto>>(url);
        return cars!.Select(car => car.Id).ToList();
    }

    private static async Task<TestUser> RegisterAsync(CarRentalApiFactory factory, string role)
    {
        var username = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        var anonymous = factory.CreateClient();

        var register = await anonymous.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            password = Password,
            role,
            fullName = "Test User",
            address = "Calle 1",
            email
        });
        register.EnsureSuccessStatusCode();

        var login = await anonymous.PostAsJsonAsync("/api/auth/login", new { username, password = Password });
        login.EnsureSuccessStatusCode();
        var auth = (await login.Content.ReadFromJsonAsync<AuthResultDto>())!;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        return new TestUser(client, username, email);
    }
}
