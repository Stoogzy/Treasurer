using FluentValidation;
using FluentValidation.Results;
using Treasurer.Application.Common.AppRequests;

namespace Treasurer.Application.Common.Behaviours;

internal sealed class ValidationHandlerDecorator<TRequest, TResult>(
    IRequestHandler<TRequest, TResult> inner,
    IEnumerable<IValidator<TRequest>> validators)
    : IRequestHandler<TRequest, TResult>
{
    public async Task<AppResponse<TResult>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        Dictionary<string, string[]>? errors = await ValidationHelper.ValidateAsync(validators, request, cancellationToken);

        if (errors is not null)
        {
            return new AppResponse<TResult>(400, default, "One or more validation errors occurred.") { Errors = errors };
        }

        return await inner.Handle(request, cancellationToken);
    }
}

internal sealed class ValidationHandlerDecorator<TRequest>(
    IRequestHandler<TRequest> inner,
    IEnumerable<IValidator<TRequest>> validators)
    : IRequestHandler<TRequest>
{
    public async Task<AppResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        Dictionary<string, string[]>? errors = await ValidationHelper.ValidateAsync(validators, request, cancellationToken);

        return errors is not null ? AppResponse.ValidationFailed(errors) : await inner.Handle(request, cancellationToken);
    }
}

internal static class ValidationHelper
{
    // Returns null when valid, otherwise errors grouped by property name.
    public static async Task<Dictionary<string, string[]>?> ValidateAsync<TRequest>(
        IEnumerable<IValidator<TRequest>> validators,
        TRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult[] results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        List<ValidationFailure> failures = results.SelectMany(r => r.Errors).ToList();

        if (failures.Count == 0)
        {
            return null;
        }

        return failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());
    }
}
