using System.Net;
using System.Net.Http.Json;
using CarRental.Application.Common.Models;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests;

public class AuthorizationTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    [Fact]
    public async Task Given_ACustomer_When_CreatingACar_Then_Returns403()
    {
        // Given
        var customer = await factory.RegisterCustomerAsync();

        // When
        var response = await customer.Client.PostAsJsonAsync("/api/cars", new { type = "Sedan", model = "Corolla" });

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Given_ACustomer_When_CheckingAvailability_Then_Returns200()
    {
        // Given
        var customer = await factory.RegisterCustomerAsync();

        // When
        var response = await customer.Client.GetAsync(
            $"/api/cars/availability?type=Sedan&startDate={TestApi.FutureDate(5):yyyy-MM-dd}&endDate={TestApi.FutureDate(7):yyyy-MM-dd}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Given_ACustomer_When_ListingCustomers_Then_Returns403()
    {
        // Given
        var customer = await factory.RegisterCustomerAsync();

        // When
        var response = await customer.Client.GetAsync("/api/customers");

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Given_ACustomer_When_RegisteringAStandaloneCustomerProfile_Then_Returns403()
    {
        // Given
        var customer = await factory.RegisterCustomerAsync();

        // When
        var response = await customer.Client.PostAsJsonAsync(
            "/api/customers", new { fullName = "Otra Persona", address = "Calle 2", email = "otra@example.com" });

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Given_ACustomer_When_ReadingARentalById_Then_Returns403()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());
        var booking = await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(12));
        var rental = (await booking.Content.ReadFromJsonAsync<RentalDto>())!;

        // When
        var response = await customer.Client.GetAsync($"/api/rentals/{rental.Id}");

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Given_AnEmployee_When_ListingCustomers_Then_SeesTheProfileCreatedByACustomerRegistration()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();

        // When
        var response = await employee.Client.GetAsync("/api/customers");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        Assert.Contains(customers!, listed => listed.Email == customer.Email);
    }
}
