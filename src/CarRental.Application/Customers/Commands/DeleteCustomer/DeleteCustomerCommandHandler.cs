using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;
using MediatR;

namespace CarRental.Application.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IRentalRepository rentalRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCustomerCommand>
{
    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        if (await rentalRepository.ExistsForCustomerAsync(customer.Id, cancellationToken))
            throw new CustomerInUseException(customer.Id);

        customerRepository.Remove(customer);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
