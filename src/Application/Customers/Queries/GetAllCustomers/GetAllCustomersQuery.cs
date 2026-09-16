using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery : IRequest<IReadOnlyList<CustomerDto>>;
