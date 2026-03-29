using Application.Product.Queries;
using FluentValidation;

namespace Application.Product.Validators;

public class GetProductDetailsQueryValidator : AbstractValidator<GetProductDetailsQuery>
{
    public GetProductDetailsQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product Id is required");
    }
}