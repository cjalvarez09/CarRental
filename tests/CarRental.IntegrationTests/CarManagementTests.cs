using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CarRental.Application.Common.Models;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests;

public class CarManagementTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    [Fact]
    public async Task Given_AnEmployee_When_CreatingACarWithEmptyFields_Then_Returns400WithFieldErrors()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();

        // When
        var response = await employee.Client.PostAsJsonAsync("/api/cars", new { type = "", model = "" });

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("Type", out _));
        Assert.True(errors.TryGetProperty("Model", out _));
    }

    [Fact]
    public async Task Given_AnUnknownCarId_When_GettingIt_Then_Returns404WithProblemDetails()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();

        // When
        var response = await employee.Client.GetAsync("/api/cars/999999");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(404, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Given_ACachedCar_When_ItIsUpdated_Then_TheNextReadReturnsTheNewValues()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType(), "Corolla");
        await employee.Client.GetFromJsonAsync<CarDto>($"/api/cars/{car.Id}");

        // When
        var update = await employee.Client.PutAsJsonAsync($"/api/cars/{car.Id}", new { type = car.Type, model = "Yaris" });
        var reread = await employee.Client.GetFromJsonAsync<CarDto>($"/api/cars/{car.Id}");

        // Then
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Yaris", reread!.Model);
    }

    [Fact]
    public async Task Given_AnUnusedCar_When_ItIsDeleted_Then_ItDisappearsFromEveryQuery()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var type = TestApi.UniqueCarType();
        var car = await employee.Client.CreateCarAsync(type);
        var start = TestApi.FutureDate(50);
        var end = TestApi.FutureDate(52);
        Assert.Contains(car.Id, await employee.Client.AvailableCarIdsAsync(type, start, end));
        Assert.Contains(car.Id, (await employee.Client.GetFromJsonAsync<List<CarDto>>("/api/cars"))!.Select(c => c.Id));

        // When
        var delete = await employee.Client.DeleteAsync($"/api/cars/{car.Id}");

        // Then
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await employee.Client.GetAsync($"/api/cars/{car.Id}")).StatusCode);
        Assert.DoesNotContain(car.Id, (await employee.Client.GetFromJsonAsync<List<CarDto>>("/api/cars"))!.Select(c => c.Id));
        Assert.DoesNotContain(car.Id, await employee.Client.AvailableCarIdsAsync(type, start, end));
    }

    [Fact]
    public async Task Given_ACarWithARental_When_DeletingIt_Then_Returns409AndTheCarRemains()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());
        (await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(12))).EnsureSuccessStatusCode();

        // When
        var delete = await employee.Client.DeleteAsync($"/api/cars/{car.Id}");

        // Then
        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await employee.Client.GetAsync($"/api/cars/{car.Id}")).StatusCode);
    }

    [Fact]
    public async Task Given_ACustomerWithARental_When_AnEmployeeDeletesThatCustomer_Then_Returns409()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var customerId = await employee.Client.GetCustomerIdAsync(customer.Email);
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());
        (await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(12))).EnsureSuccessStatusCode();

        // When
        var delete = await employee.Client.DeleteAsync($"/api/customers/{customerId}");

        // Then
        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
    }
}
