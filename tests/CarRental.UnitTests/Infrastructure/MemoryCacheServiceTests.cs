using CarRental.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace CarRental.UnitTests.Infrastructure;

public class MemoryCacheServiceTests : IDisposable
{
    private static readonly TimeSpan LongExpiration = TimeSpan.FromMinutes(5);

    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly MemoryCacheService _service;

    public MemoryCacheServiceTests()
    {
        _service = new MemoryCacheService(_memoryCache);
    }

    public void Dispose() => _memoryCache.Dispose();

    private static Func<Task<string>> CountingFactory(Counter counter, string value = "value") =>
        () =>
        {
            counter.Value++;
            return Task.FromResult(value);
        };

    private sealed class Counter
    {
        public int Value { get; set; }
    }

    [Fact]
    public async Task Given_AnEmptyCache_When_GettingAKey_Then_RunsTheFactoryAndReturnsItsValue()
    {
        // Given
        var counter = new Counter();

        // When
        var result = await _service.GetOrCreateAsync("key", ["tag"], LongExpiration, CountingFactory(counter, "fresh"));

        // Then
        Assert.Equal("fresh", result);
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public async Task Given_ACachedKey_When_GettingItAgain_Then_ReturnsTheCachedValueWithoutRunningTheFactory()
    {
        // Given
        var counter = new Counter();
        await _service.GetOrCreateAsync("key", ["tag"], LongExpiration, CountingFactory(counter, "first"));

        // When
        var result = await _service.GetOrCreateAsync("key", ["tag"], LongExpiration, CountingFactory(counter, "second"));

        // Then
        Assert.Equal("first", result);
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public async Task Given_DifferentKeys_When_Getting_Then_EachIsCachedIndependently()
    {
        // Given
        var counter = new Counter();

        // When
        await _service.GetOrCreateAsync("key-a", ["tag"], LongExpiration, CountingFactory(counter));
        await _service.GetOrCreateAsync("key-b", ["tag"], LongExpiration, CountingFactory(counter));

        // Then
        Assert.Equal(2, counter.Value);
    }

    [Fact]
    public async Task Given_SeveralEntriesUnderATag_When_InvalidatingTheTag_Then_AllOfThemAreEvicted()
    {
        // Given
        var counter = new Counter();
        await _service.GetOrCreateAsync("key-a", ["cars"], LongExpiration, CountingFactory(counter));
        await _service.GetOrCreateAsync("key-b", ["cars"], LongExpiration, CountingFactory(counter));

        // When
        _service.Invalidate("cars");

        // Then
        await _service.GetOrCreateAsync("key-a", ["cars"], LongExpiration, CountingFactory(counter));
        await _service.GetOrCreateAsync("key-b", ["cars"], LongExpiration, CountingFactory(counter));
        Assert.Equal(4, counter.Value);
    }

    [Fact]
    public async Task Given_EntriesUnderDifferentTags_When_InvalidatingOne_Then_TheOthersAreUntouched()
    {
        // Given
        var counter = new Counter();
        await _service.GetOrCreateAsync("cars-key", ["cars"], LongExpiration, CountingFactory(counter));
        await _service.GetOrCreateAsync("customers-key", ["customers"], LongExpiration, CountingFactory(counter));

        // When
        _service.Invalidate("cars");

        // Then
        await _service.GetOrCreateAsync("customers-key", ["customers"], LongExpiration, CountingFactory(counter));
        Assert.Equal(2, counter.Value);
    }

    [Fact]
    public async Task Given_AnEntryWithSeveralTags_When_InvalidatingAnyOfThem_Then_ItIsEvicted()
    {
        // Given
        var counter = new Counter();
        await _service.GetOrCreateAsync("availability", ["cars", "rentals"], LongExpiration, CountingFactory(counter));

        // When
        _service.Invalidate("rentals");

        // Then
        await _service.GetOrCreateAsync("availability", ["cars", "rentals"], LongExpiration, CountingFactory(counter));
        Assert.Equal(2, counter.Value);
    }

    [Fact]
    public async Task Given_ATagInvalidatedMoreThanOnce_When_CachingAgain_Then_KeepsWorkingAfterEachInvalidation()
    {
        // Given
        var counter = new Counter();

        // When
        await _service.GetOrCreateAsync("key", ["cars"], LongExpiration, CountingFactory(counter));
        _service.Invalidate("cars");
        await _service.GetOrCreateAsync("key", ["cars"], LongExpiration, CountingFactory(counter));
        _service.Invalidate("cars");
        await _service.GetOrCreateAsync("key", ["cars"], LongExpiration, CountingFactory(counter));

        // Then
        Assert.Equal(3, counter.Value);
    }

    [Fact]
    public void Given_AnUnknownTag_When_Invalidating_Then_DoesNotThrow()
    {
        // Given
        const string unknownTag = "never-used";

        // When
        var exception = Record.Exception(() => _service.Invalidate(unknownTag));

        // Then
        Assert.Null(exception);
    }

    [Fact]
    public async Task Given_AnExpiredEntry_When_GettingItAgain_Then_RunsTheFactoryAgain()
    {
        // Given
        var counter = new Counter();
        var shortExpiration = TimeSpan.FromMilliseconds(50);
        await _service.GetOrCreateAsync("key", ["tag"], shortExpiration, CountingFactory(counter));
        await Task.Delay(300);

        // When
        await _service.GetOrCreateAsync("key", ["tag"], shortExpiration, CountingFactory(counter));

        // Then
        Assert.Equal(2, counter.Value);
    }
}
