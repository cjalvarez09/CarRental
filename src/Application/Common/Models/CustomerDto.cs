using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Models;

public record CustomerDto(int Id, string FullName, string Address, string Email);

public static class CustomerMappingExtensions
{
    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.FullName, customer.Address, customer.Email);
}
