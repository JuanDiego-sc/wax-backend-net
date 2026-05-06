using Application.Interfaces.DTOs;
using Application.Product.Commands;
using Application.Product.DTOs;
using Application.Product.Extensions;
using Application.Product.Queries;
using Domain.Enumerators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), 200)]
    public async Task<ActionResult<List<ProductDto>>> GetProducts([FromQuery] ProductParams productParams)
    {
        return await HandlePagedQuery(new GetProductsQuery { ProductParams = productParams });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ProductDto>> GetProductDetails(string id)
    {
        return await HandleQuery(new GetProductDetailsQuery { Id = id });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto productDto, IFormFile file )
    {
        ImageUploadRequest? imageUploadRequest = null;
        imageUploadRequest = new ImageUploadRequest(file.OpenReadStream(), file.FileName, file.ContentType);
        
        return await HandleCommand(new CreateProductCommand { ProductDto = productDto,  ImageRequest = imageUploadRequest! });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> UpdateProduct([FromForm] UpdateProductDto productDto, IFormFile? file)
    {
        ImageUploadRequest? imageUploadRequest = null;
        if(file != null)
            imageUploadRequest = new ImageUploadRequest(file!.OpenReadStream(), file.FileName, file.ContentType);
        
        return await HandleCommand(new UpdateProductCommand { ProductDto = productDto, ImageRequest = imageUploadRequest });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> DeleteProduct(string id)
    {
        return await HandleCommand(new DeleteProductCommand { ProductId = id });
    }
}
