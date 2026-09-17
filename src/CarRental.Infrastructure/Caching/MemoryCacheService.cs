using System.Collections.Concurrent;
using CarRental.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace CarRental.Infrastructure.Caching;

public class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tagTokens = new();

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        IReadOnlyCollection<string> tags,
        TimeSpan expiration,
        Func<Task<T>> factory)
    {
        if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();

        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiration);

        foreach (var tag in tags)
        {
            var tokenSource = _tagTokens.GetOrAdd(tag, _ => new CancellationTokenSource());
            options.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
        }

        memoryCache.Set(key, value, options);

        return value;
    }

    public void Invalidate(string tag)
    {
        if (!_tagTokens.TryRemove(tag, out var tokenSource))
            return;

        tokenSource.Cancel();
        tokenSource.Dispose();
    }
}
