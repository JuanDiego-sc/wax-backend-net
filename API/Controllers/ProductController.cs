using Application.Interfaces.DTOs;
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
        return await HandleQuery(new GetProductsQuery { ProductParams = productParams });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductDetails(string id)
    {
        return await HandleQuery(new GetProductDetailsQuery { Id = id });
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto productDto, IFormFile file )
    {
        ImageUploadRequest? imageUploadRequest = null;
        imageUploadRequest = new ImageUploadRequest(file.OpenReadStream(), file.FileName, file.ContentType);
        
        return await HandleCommand(new CreateProductCommand { ProductDto = productDto,  ImageRequest = imageUploadRequest! });
    }

    [HttpPut]
    public async Task<ActionResult> UpdateProduct(UpdateProductDto productDto, IFormFile? file)
    {
        ImageUploadRequest? imageUploadRequest = null;
        imageUploadRequest = new ImageUploadRequest(file!.OpenReadStream(), file.FileName, file.ContentType);
        
        return await HandleCommand(new UpdateProductCommand { ProductDto = productDto, ImageRequest = imageUploadRequest });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(string id)
    {
        return await HandleCommand(new DeleteProductCommand { ProductId = id });
    }
}
