using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Mappings;

public static class CarMappingExtensions
{
    public static CarDto ToDto(this Car car) => new(car.Id, car.Type, car.Model);
}
