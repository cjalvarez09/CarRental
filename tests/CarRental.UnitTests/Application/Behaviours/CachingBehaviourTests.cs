using CarRental.Application.Common.Behaviours;
using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Interfaces;
using MediatR;

namespace CarRental.UnitTests.Application.Behaviours;

public class CachingBehaviourTests
{
    public record NonCacheableRequest : IRequest<string>;

    public record CacheableRequest : IRequest<string>, ICacheableQuery
    {
        public string CacheKey => "sample:key";

        public IReadOnlyCollection<string> CacheTags => ["sample"];
    }

    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();

    [Fact]
    public async Task Given_ANonCacheableRequest_When_Handling_Then_CallsNextAndBypassesTheCache()
    {
        // Given
        var behaviour = new CachingBehaviour<NonCacheableRequest, string>(_cacheService);

        // When
        var result = await behaviour.Handle(new NonCacheableRequest(), () => Task.FromResult("fresh"), CancellationToken.None);

        // Then
        Assert.Equal("fresh", result);
        await _cacheService.DidNotReceiveWithAnyArgs()
            .GetOrCreateAsync<string>(default!, default!, default, default!);
    }

    [Fact]
    public async Task Given_ACacheHit_When_Handling_Then_ReturnsTheCachedValueWithoutCallingNext()
    {
        // Given
        _cacheService
            .GetOrCreateAsync(
                "sample:key",
                Arg.Any<IReadOnlyCollection<string>>(),
                TimeSpan.FromMinutes(5),
                Arg.Any<Func<Task<string>>>())
            .Returns("cached");
        var nextWasCalled = false;
        var behaviour = new CachingBehaviour<CacheableRequest, string>(_cacheService);

        // When
        var result = await behaviour.Handle(
            new CacheableRequest(),
            () =>
            {
                nextWasCalled = true;
                return Task.FromResult("fresh");
            },
            CancellationToken.None);

        // Then
        Assert.Equal("cached", result);
        Assert.False(nextWasCalled);
    }

    [Fact]
    public async Task Given_ACacheMiss_When_Handling_Then_TheFactoryRunsTheRestOfThePipeline()
    {
        // Given
        _cacheService
            .GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyCollection<string>>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<Func<Task<string>>>())
            .Returns(call => call.Arg<Func<Task<string>>>()());
        var behaviour = new CachingBehaviour<CacheableRequest, string>(_cacheService);

        // When
        var result = await behaviour.Handle(new CacheableRequest(), () => Task.FromResult("fresh"), CancellationToken.None);

        // Then
        Assert.Equal("fresh", result);
    }

    [Fact]
    public async Task Given_ACacheableRequest_When_Handling_Then_UsesItsKeyTagsAndDefaultExpiration()
    {
        // Given
        _cacheService
            .GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyCollection<string>>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<Func<Task<string>>>())
            .Returns("value");
        var behaviour = new CachingBehaviour<CacheableRequest, string>(_cacheService);

        // When
        await behaviour.Handle(new CacheableRequest(), () => Task.FromResult("fresh"), CancellationToken.None);

        // Then
        await _cacheService.Received(1).GetOrCreateAsync(
            "sample:key",
            Arg.Is<IReadOnlyCollection<string>>(tags => tags.SequenceEqual(new[] { "sample" })),
            TimeSpan.FromMinutes(5),
            Arg.Any<Func<Task<string>>>());
    }
}
