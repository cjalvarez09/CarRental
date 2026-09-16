using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Persistence.Repositories;

public class RentalRepository(CarRentalDbContext dbContext) : IRentalRepository
{
    public Task<Rental?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Rentals
            .Include(rental => rental.Customer)
            .Include(rental => rental.Car)
            .FirstOrDefaultAsync(rental => rental.Id == id, cancellationToken);

    public Task<bool> HasOverlapAsync(
        int carId,
        DateTime startDate,
        DateTime endDate,
        int? excludeRentalId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.Rentals.AnyAsync(rental =>
            rental.CarId == carId
            && rental.Status == RentalStatus.Active
            && (excludeRentalId == null || rental.Id != excludeRentalId.Value)
            && rental.StartDate < endDate
            && startDate < rental.EndDate,
            cancellationToken);

    public Task<bool> ExistsForCarAsync(int carId, CancellationToken cancellationToken = default) =>
        dbContext.Rentals.AnyAsync(rental => rental.CarId == carId, cancellationToken);

    public Task<bool> ExistsForCustomerAsync(int customerId, CancellationToken cancellationToken = default) =>
        dbContext.Rentals.AnyAsync(rental => rental.CustomerId == customerId, cancellationToken);

    public async Task AddAsync(Rental rental, CancellationToken cancellationToken = default) =>
        await dbContext.Rentals.AddAsync(rental, cancellationToken);
}
