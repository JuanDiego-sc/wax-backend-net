using Application.Product.Queries;
using FluentValidation;

namespace Application.Product.Validators;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.ProductParams)
            .NotNull().WithMessage("Product params is required.");
        When(x => x.ProductParams != null, () =>
        {
            RuleFor(x => x.ProductParams.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than zero.");
            RuleFor(x => x.ProductParams.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than zero.");

            RuleFor(x => x.ProductParams.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than zero.")
                .LessThanOrEqualTo(50).WithMessage("PageSize must be less than or equal to 50.");
        });
    }
}