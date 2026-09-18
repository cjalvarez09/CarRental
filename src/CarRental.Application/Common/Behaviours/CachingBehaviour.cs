using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Interfaces;
using MediatR;

namespace CarRental.Application.Common.Behaviours;

public class CachingBehaviour<TRequest, TResponse>(ICacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
            return next();

        return cacheService.GetOrCreateAsync(
            cacheableQuery.CacheKey,
            cacheableQuery.CacheTags,
            cacheableQuery.Expiration,
            () => next());
    }
}
