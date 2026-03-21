using FluentValidation;

namespace Application.Product.Validators;

public class UpdateProductValidator : AbstractValidator<Commands.UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ProductDto.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ProductDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.ProductDto.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.ProductDto.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.ProductDto.Type)
            .NotEmpty().WithMessage("Type is required.");

        RuleFor(x => x.ProductDto.Brand)
            .NotEmpty().WithMessage("Brand is required.");

        RuleFor(x => x.ProductDto.QuantityInStock)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity in stock cannot be negative.")
            .LessThanOrEqualTo(200).WithMessage("Quantity in stock must not exceed 200.");
    }
}
