using FluentValidation;
using Application.Payment.Commands;

namespace Application.Payment.Validators;

public class HandleStripeWebhookValidator : AbstractValidator<HandleStripeWebhookCommand>
{
    public HandleStripeWebhookValidator()
    {
        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload is required.");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature is required.");
    }
}
