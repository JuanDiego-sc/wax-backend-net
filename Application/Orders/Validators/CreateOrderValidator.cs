using FluentValidation;
using Application.Orders.Commands;

namespace Application.Orders.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.BasketId)
            .NotEmpty().WithMessage("BasketId is required.");

        RuleFor(x => x.OrderDto.PaymentSummary)
            .NotNull().WithMessage("PaymentSummary is required.");

        When(x => x.OrderDto.PaymentSummary != null, () =>
        {
            RuleFor(x => x.OrderDto.PaymentSummary.Brand)
                .NotEmpty().WithMessage("PaymentSummary.Brand is required.");

            RuleFor(x => x.OrderDto.PaymentSummary.Last4)
                .InclusiveBetween(1000, 9999).WithMessage("PaymentSummary.Last4 must be a 4-digit number.");

            RuleFor(x => x.OrderDto.PaymentSummary.ExpMonth)
                .InclusiveBetween(1, 12).WithMessage("PaymentSummary.ExpMonth must be between 1 and 12.");

            RuleFor(x => x.OrderDto.PaymentSummary.ExpYear)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
                .WithMessage("PaymentSummary.ExpYear must not be in the past.");
        });
    }
}
