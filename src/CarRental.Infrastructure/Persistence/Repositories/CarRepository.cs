using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Persistence.Repositories;

public class CarRepository(CarRentalDbContext dbContext) : ICarRepository
{
    public Task<Car?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Cars.FirstOrDefaultAsync(car => car.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Car>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Cars.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Car>> GetAvailableAsync(
        string type,
        string? model,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Cars.Where(car => car.Type == type);

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(car => car.Model == model);

        query = query.Where(car => !dbContext.Rentals.Any(rental =>
            rental.CarId == car.Id
            && rental.Status == RentalStatus.Active
            && rental.StartDate < endDate
            && startDate < rental.EndDate));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Car car, CancellationToken cancellationToken = default) =>
        await dbContext.Cars.AddAsync(car, cancellationToken);

    public void Remove(Car car)
    {
        car.IsDeleted = true;
        car.DeletedAtUtc = DateTime.UtcNow;
    }
}
