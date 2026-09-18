using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Mappings;

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
