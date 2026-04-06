using Application.Product.Queries;
using Application.Product.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Product.Validators;

public class GetProductDetailsQueryValidatorTests
{
    private readonly GetProductDetailsQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenIdEmpty_HasError()
    {
        var query = new GetProductDetailsQuery { Id = "" };

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_WhenIdNull_HasError()
    {
        var query = new GetProductDetailsQuery { Id = null! };

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_WhenIdValid_NoErrors()
    {
        var query = new GetProductDetailsQuery { Id = "product-123" };

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
