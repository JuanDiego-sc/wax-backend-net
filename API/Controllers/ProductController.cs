using System;
using Application.Product.Commands;
using Application.Product.DTOs;
using Application.Product.Extensions;
using Application.Product.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts([FromQuery] ProductParams productParams)
    {
        var query = new GetProductsQuery { ProductParams = productParams };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductDetails(string id)
    {
        var query = new GetProductDetailsQuery { Id = id };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto productDto)
    {
        var command = new CreateProductCommand { ProductDto =  productDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateProduct(UpdateProductDto productDto)
    {
        var command = new UpdateProductCommand { ProductDto = productDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(string id)
    {
        var command = new DeleteProductCommand { ProductId = id };
        return HandleResult(await Mediator.Send(command));
    }
    
}
