using Application.Product.DTOs;
using ProductDomain = Domain.Entities.Product;

namespace Application.Product.Extensions;

public static class ProductExtensions
{
    public static ProductDto ToDto(this ProductDomain product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            PictureUrl = product.PictureUrl,
            Type = product.Type,
            Brand = product.Brand,
            QuantityInStock = product.QuantityInStock,
            PublicId =  product.PublicId
        };
    }

    public static IQueryable<ProductDto> Sort(this IQueryable<ProductDto> query, string? orderBy)
    {
        query = orderBy switch
            {
                "price" => query.OrderBy(x => x.Price), //lower to highest 
                "priceDesc" => query.OrderByDescending(x => x.Price), //highest to lower
                _ => query.OrderBy(x => x.Name)

            };
        return query;
    }

    public static IQueryable<ProductDto> Search(this IQueryable<ProductDto> query, string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return query;

        var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(x => x.Name.ToLower().Contains(lowerCaseSearchTerm));
    }

    public static IQueryable<ProductDto> Filter(this IQueryable<ProductDto> query, string? brands, string? types)
    {
        var brandList = new List<string>();
        var typeList = new List<string>();

        if (!string.IsNullOrEmpty(brands))
        {
            brandList.AddRange([.. brands.ToLower().Split(",")]);
        }

        if (!string.IsNullOrEmpty(types))
        {
            typeList.AddRange([.. types.ToLower().Split(",")]);
        }

        query = query.Where(x => brandList.Count == 0 || brandList.Contains(x.Brand.ToLower()));
        query = query.Where(x => typeList.Count == 0 || typeList.Contains(x.Type.ToLower()));

        return query;
    }
}
