using System;
using Application.Product.DTOs;
using ProductDomain = Domain.Entities.Product;

namespace Application.Product.Extensions;

public static class ProductExtensions
{
    public static ProductDto ToDto(this ProductDomain product)
    {
        return new ProductDto
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            PictureUrl = product.PictureUrl,
            Type = product.Type,
            Brand = product.Brand,
            QuantityInStock = product.QuantityInStock
        };
    }

    public static IQueryable<ProductDomain> Sort(this IQueryable<ProductDomain> query, string? orderBy)
    {
        query = orderBy switch
            {
                "price" => query.OrderBy(x => x.Price), //lower to highest 
                "priceDesc" => query.OrderByDescending(x => x.Price), //highest to lower
                _ => query.OrderBy(x => x.Name)

            };
        return query;
    }

    public static IQueryable<ProductDomain> Search(this IQueryable<ProductDomain> query, string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return query;

        var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(x => x.Name.ToLower().Contains(lowerCaseSearchTerm));
    }

    public static IQueryable<ProductDomain> Filter(this IQueryable<ProductDomain> query, string? brands, string? types)
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
