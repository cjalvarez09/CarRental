using CarRental.Domain.Entities;

namespace CarRental.Domain.Repositories;

public interface ICarRepository
{
    Task<Car?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Car>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Car>> GetAvailableAsync(
        string type,
        string? model,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(Car car, CancellationToken cancellationToken = default);

    void Remove(Car car);
}
