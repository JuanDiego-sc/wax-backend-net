using Application.Core.Validations;
using Domain.ProductAggregate;
using MediatR;

namespace Application.Product.Commands.Delete;

public interface IProductDeletionStrategy
{
    string Kind { get; }
    Task<Result<Unit>> ExecuteAsync(Domain.ProductAggregate.Product product, CancellationToken cancellationToken);
}
