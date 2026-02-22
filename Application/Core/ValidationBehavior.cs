using System;
using FluentValidation;
using MediatR;

namespace Application.Core;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null) :
    IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validator == null) return await next(cancellationToken);

        var valdiationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valdiationResult.IsValid)
        {
            throw new ValidationException(valdiationResult.Errors);
        }

        return await next(cancellationToken);
    }
}
