namespace CarRental.Application.Common.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }

    IReadOnlyCollection<string> CacheTags { get; }

    TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
