using CarRental.Application.Common.Models;
using MediatR;

namespace CarRental.Application.Customers.Commands.RegisterCustomer;

public record RegisterCustomerCommand(string FullName, string Address, string Email) : IRequest<CustomerDto>;
