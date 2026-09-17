namespace CarRental.Application.Common.Interfaces;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        IReadOnlyCollection<string> tags,
        TimeSpan expiration,
        Func<Task<T>> factory);

    void Invalidate(string tag);
}
