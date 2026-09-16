using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Persistence.Repositories;

public class CustomerRepository(CarRentalDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Customers.ToListAsync(cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await dbContext.Customers.AddAsync(customer, cancellationToken);

    public void Remove(Customer customer) => dbContext.Customers.Remove(customer);
}
