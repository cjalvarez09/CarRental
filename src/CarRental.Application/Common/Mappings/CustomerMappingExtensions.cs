using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Mappings;

public static class CustomerMappingExtensions
{
    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.FullName, customer.Address, customer.Email);
}
