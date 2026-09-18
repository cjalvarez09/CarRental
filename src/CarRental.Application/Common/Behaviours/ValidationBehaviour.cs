using FluentValidation;
using MediatR;

namespace CarRental.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        // Each validator needs its own context: a shared one accumulates failures across validators,
        // so every result would also contain the failures of the others.
        var failures = (await Task.WhenAll(validators.Select(validator =>
                validator.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken))))
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
