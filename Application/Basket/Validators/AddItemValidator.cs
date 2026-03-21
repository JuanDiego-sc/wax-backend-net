using FluentValidation;

namespace Application.Basket.Validators;

public class AddItemValidator : AbstractValidator<Commands.AddItemCommand>
{
    public AddItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.BasketId)
            .NotEmpty().WithMessage("BasketId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100.");
    }
}
