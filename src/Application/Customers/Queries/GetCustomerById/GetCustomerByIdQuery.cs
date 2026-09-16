using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int CustomerId) : IRequest<CustomerDto>;
