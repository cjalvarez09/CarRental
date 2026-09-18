namespace CarRental.Application.Common.Models;

public record RentalDto(
    int Id,
    CustomerDto Customer,
    CarDto Car,
    DateTime StartDate,
    DateTime EndDate,
    string Status);
