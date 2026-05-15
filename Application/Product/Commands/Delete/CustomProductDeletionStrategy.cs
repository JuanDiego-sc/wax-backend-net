using Application.Core.Validations;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Enumerators;
using Domain.ProductAggregate;
using MediatR;

namespace Application.Product.Commands.Delete;

public class CustomProductDeletionStrategy(
    ICustomProductRepository customProductRepository) : IProductDeletionStrategy
{
    public string Kind => ProductTypes.Custom;

    public Task<Result<Unit>> ExecuteAsync(Domain.ProductAggregate.Product product, CancellationToken cancellationToken)
    {
        if (product is not CustomProduct custom)
            return Task.FromResult(Result<Unit>.Failure($"Product '{product.Id}' is not a custom product"));

        customProductRepository.Remove(custom);
        return Task.FromResult(Result<Unit>.Success(Unit.Value));
    }
}
