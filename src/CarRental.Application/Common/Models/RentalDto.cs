using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Models;

public record RentalDto(
    int Id,
    CustomerDto Customer,
    CarDto Car,
    DateTime StartDate,
    DateTime EndDate,
    string Status);

public static class RentalMappingExtensions
{
    public static RentalDto ToDto(this Rental rental) => new(
        rental.Id,
        rental.Customer.ToDto(),
        rental.Car.ToDto(),
        rental.StartDate,
        rental.EndDate,
        rental.Status.ToString());
}
