using FluentValidation;
using RegisterPanel.Application.Common;
using MediatR;
using System.Reflection;

namespace RegisterPanel.Application.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        ValidationContext<TRequest> context = new(request);

        FluentValidation.Results.ValidationResult[] results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<FluentValidation.Results.ValidationFailure> failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        string errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
        return CreateFailureResult(errorMessage);
    }

    private static TResponse CreateFailureResult(string errorMessage)
    {
        Type responseType = typeof(TResponse);

        if (responseType == typeof(Result))
            return (TResponse)(object)Result.Failure("VALIDATION_ERROR", errorMessage);

        MethodInfo? failureMethod = responseType.GetMethod(
            "Failure",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new Type[] { typeof(string), typeof(string) },
            null);

        if (failureMethod is null)
            return (TResponse)(object)Result.Failure("VALIDATION_ERROR", errorMessage);

        return (TResponse)failureMethod.Invoke(null, new object[] { "VALIDATION_ERROR", errorMessage })!;
    }
}
