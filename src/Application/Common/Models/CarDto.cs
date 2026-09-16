using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Models;

public record CarDto(int Id, string Type, string Model);

public static class CarMappingExtensions
{
    public static CarDto ToDto(this Car car) => new(car.Id, car.Type, car.Model);
}
