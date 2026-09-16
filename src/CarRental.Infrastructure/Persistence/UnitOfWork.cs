using CarRental.Application.Common.Interfaces;

namespace CarRental.Infrastructure.Persistence;

public class UnitOfWork(CarRentalDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
