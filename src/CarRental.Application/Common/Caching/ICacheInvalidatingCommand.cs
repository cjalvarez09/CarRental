namespace CarRental.Application.Common.Caching;

public interface ICacheInvalidatingCommand
{
    IReadOnlyCollection<string> CacheTagsToInvalidate { get; }
}
