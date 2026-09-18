using CarRental.Application.Common.Caching;
using CarRental.Application.Common.Interfaces;
using MediatR;

namespace CarRental.Application.Common.Behaviours;

public class CacheInvalidationBehaviour<TRequest, TResponse>(ICacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICacheInvalidatingCommand invalidatingCommand)
        {
            foreach (var tag in invalidatingCommand.CacheTagsToInvalidate)
                cacheService.Invalidate(tag);
        }

        return response;
    }
}
