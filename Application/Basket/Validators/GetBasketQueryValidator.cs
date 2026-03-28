using Application.Basket.Queries;
using FluentValidation;

namespace Application.Basket.Validators;

public class GetBasketQueryValidator : AbstractValidator<GetBasketQuery>
{
    public GetBasketQueryValidator()
    {
        RuleFor(basketQuery => basketQuery.BasketId)
            .NotEmpty().WithMessage("BasketId is required.");
    }
}