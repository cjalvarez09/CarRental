using MediatR;

namespace CarRental.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int CustomerId) : IRequest;
