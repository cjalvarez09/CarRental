using CarRental.Domain.Entities;

namespace CarRental.Domain.Repositories;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        int carId,
        DateTime startDate,
        DateTime endDate,
        int? excludeRentalId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForCarAsync(int carId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task AddAsync(Rental rental, CancellationToken cancellationToken = default);
}
