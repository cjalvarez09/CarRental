using System.Net;
using System.Net.Http.Json;
using CarRental.Application.Common.Models;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests;

public class RentalFlowTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    [Fact]
    public async Task Given_ACustomerAndAnAvailableCar_When_TheCustomerBooks_Then_TheRentalIsCreatedForThem()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var customerId = await employee.Client.GetCustomerIdAsync(customer.Email);
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());

        // When
        var response = await customer.Client.BookAsync(customerId, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(14));

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var rental = await response.Content.ReadFromJsonAsync<RentalDto>();
        Assert.Equal("Active", rental!.Status);
        Assert.Equal(customerId, rental.Customer.Id);
        Assert.Equal(car.Id, rental.Car.Id);
    }

    [Fact]
    public async Task Given_ACustomer_When_BookingWithAnotherCustomersId_Then_TheRentalStillBelongsToTheCaller()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var caller = await factory.RegisterCustomerAsync();
        var other = await factory.RegisterCustomerAsync();
        var callerId = await employee.Client.GetCustomerIdAsync(caller.Email);
        var otherId = await employee.Client.GetCustomerIdAsync(other.Email);
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());

        // When
        var response = await caller.Client.BookAsync(otherId, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(14));

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var rental = await response.Content.ReadFromJsonAsync<RentalDto>();
        Assert.Equal(callerId, rental!.Customer.Id);
        Assert.NotEqual(otherId, rental.Customer.Id);
    }

    [Fact]
    public async Task Given_AnEmployee_When_BookingForACustomer_Then_TheRentalBelongsToThatCustomer()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var customerId = await employee.Client.GetCustomerIdAsync(customer.Email);
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());

        // When
        var response = await employee.Client.BookAsync(customerId, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(14));

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var rental = await response.Content.ReadFromJsonAsync<RentalDto>();
        Assert.Equal(customerId, rental!.Customer.Id);
    }

    [Fact]
    public async Task Given_ABookedCar_When_BookingOverlappingDates_Then_Returns409()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());
        var firstBooking = await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(15));
        firstBooking.EnsureSuccessStatusCode();

        // When
        var overlapping = await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(12), TestApi.FutureDate(18));

        // Then
        Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
    }

    [Fact]
    public async Task Given_AnAvailableCar_When_ItIsBookedAndThenCancelled_Then_AvailabilityReflectsEachChange()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var type = TestApi.UniqueCarType();
        var car = await employee.Client.CreateCarAsync(type);
        var start = TestApi.FutureDate(20);
        var end = TestApi.FutureDate(25);
        var availableBefore = await customer.Client.AvailableCarIdsAsync(type, start, end);

        // When
        var booking = await customer.Client.BookAsync(1, car.Id, start, end);
        var rental = (await booking.Content.ReadFromJsonAsync<RentalDto>())!;
        var availableWhileBooked = await customer.Client.AvailableCarIdsAsync(type, start, end);
        var cancel = await employee.Client.PostAsync($"/api/rentals/{rental.Id}/cancel", content: null);
        var availableAfterCancel = await customer.Client.AvailableCarIdsAsync(type, start, end);

        // Then
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        Assert.Contains(car.Id, availableBefore);
        Assert.DoesNotContain(car.Id, availableWhileBooked);
        Assert.Contains(car.Id, availableAfterCancel);
    }

    [Fact]
    public async Task Given_ACancelledRental_When_CancellingItAgain_Then_Returns409()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());
        var booking = await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(10), TestApi.FutureDate(12));
        var rental = (await booking.Content.ReadFromJsonAsync<RentalDto>())!;
        await employee.Client.PostAsync($"/api/rentals/{rental.Id}/cancel", content: null);

        // When
        var secondCancel = await employee.Client.PostAsync($"/api/rentals/{rental.Id}/cancel", content: null);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, secondCancel.StatusCode);
    }

    [Fact]
    public async Task Given_ARentalInThePast_When_Booking_Then_Returns400()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());

        // When
        var response = await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(-5), TestApi.FutureDate(-2));

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Given_ABookedCar_When_ModifyingTheRentalToDatesTakenByAnotherOne_Then_Returns409()
    {
        // Given
        var employee = await factory.RegisterEmployeeAsync();
        var customer = await factory.RegisterCustomerAsync();
        var car = await employee.Client.CreateCarAsync(TestApi.UniqueCarType());
        (await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(30), TestApi.FutureDate(35))).EnsureSuccessStatusCode();
        var second = await customer.Client.BookAsync(1, car.Id, TestApi.FutureDate(40), TestApi.FutureDate(45));
        var secondRental = (await second.Content.ReadFromJsonAsync<RentalDto>())!;

        // When
        var response = await employee.Client.PutAsJsonAsync(
            $"/api/rentals/{secondRental.Id}",
            new { startDate = TestApi.FutureDate(33), endDate = TestApi.FutureDate(38) });

        // Then
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
