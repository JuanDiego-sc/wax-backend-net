using FluentValidation;
using Application.Payment.Commands;

namespace Application.Payment.Validators;

public class CreateOrUpdateIntentValidator : AbstractValidator<CreateOrUpdateIntentCommand>
{
    public CreateOrUpdateIntentValidator()
    {
        RuleFor(x => x.BasketId)
            .NotEmpty().WithMessage("BasketId is required.");
    }
}
