using Application.Core.Validations;
using Application.Interfaces;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Enumerators;
using Domain.ProductAggregate;
using MediatR;

namespace Application.Product.Commands.Delete;

public class CatalogProductDeletionStrategy(
    IProductRepository productRepository,
    IImageService imageService) : IProductDeletionStrategy
{
    public string Kind => ProductTypes.Catalog;

    public async Task<Result<Unit>> ExecuteAsync(Domain.ProductAggregate.Product product, CancellationToken cancellationToken)
    {
        if (product is not CatalogProduct catalog)
            return Result<Unit>.Failure($"Product '{product.Id}' is not a catalog product");

        if (!string.IsNullOrEmpty(catalog.PublicId))
            await imageService.DeleteImage(catalog.PublicId, cancellationToken);

        productRepository.Remove(catalog);
        return Result<Unit>.Success(Unit.Value);
    }
}
