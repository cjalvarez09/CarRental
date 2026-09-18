using CarRental.Application.Common.Behaviours;
using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Interfaces;
using MediatR;

namespace CarRental.UnitTests.Application.Behaviours;

public class CacheInvalidationBehaviourTests
{
    public record PlainCommand : IRequest<string>;

    public record InvalidatingCommand : IRequest<string>, ICacheInvalidatingCommand
    {
        public IReadOnlyCollection<string> CacheTagsToInvalidate => ["cars", "rentals"];
    }

    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();

    [Fact]
    public async Task Given_ACommandDeclaringTags_When_TheHandlerSucceeds_Then_InvalidatesEachTag()
    {
        // Given
        var behaviour = new CacheInvalidationBehaviour<InvalidatingCommand, string>(_cacheService);

        // When
        var result = await behaviour.Handle(new InvalidatingCommand(), () => Task.FromResult("done"), CancellationToken.None);

        // Then
        Assert.Equal("done", result);
        _cacheService.Received(1).Invalidate("cars");
        _cacheService.Received(1).Invalidate("rentals");
    }

    [Fact]
    public async Task Given_ACommandDeclaringTags_When_TheHandlerThrows_Then_InvalidatesNothing()
    {
        // Given
        var behaviour = new CacheInvalidationBehaviour<InvalidatingCommand, string>(_cacheService);

        // When
        Func<Task> act = () => behaviour.Handle(
            new InvalidatingCommand(),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        _cacheService.DidNotReceive().Invalidate(Arg.Any<string>());
    }

    [Fact]
    public async Task Given_ACommandWithoutTags_When_Handling_Then_InvalidatesNothing()
    {
        // Given
        var behaviour = new CacheInvalidationBehaviour<PlainCommand, string>(_cacheService);

        // When
        var result = await behaviour.Handle(new PlainCommand(), () => Task.FromResult("done"), CancellationToken.None);

        // Then
        Assert.Equal("done", result);
        _cacheService.DidNotReceive().Invalidate(Arg.Any<string>());
    }
}
