using FluentValidation;
using Application.Orders.Commands;

namespace Application.Orders.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.BasketId)
            .NotEmpty().WithMessage("BasketId is required.");

        RuleFor(x => x.OrderDto.BillingAddress)
            .NotNull().WithMessage("BillingAddress is required.");

        When(x => x.OrderDto.BillingAddress != null, () =>
        {
            RuleFor(x => x.OrderDto.BillingAddress.Name)
                .NotEmpty().WithMessage("BillingAddress.Name is required.")
                .MaximumLength(100).WithMessage("BillingAddress.Name must not exceed 100 characters.");

            RuleFor(x => x.OrderDto.BillingAddress.Line1)
                .NotEmpty().WithMessage("BillingAddress.Line1 is required.")
                .MaximumLength(200).WithMessage("BillingAddress.Line1 must not exceed 200 characters.");

            RuleFor(x => x.OrderDto.BillingAddress.City)
                .NotEmpty().WithMessage("BillingAddress.City is required.")
                .MaximumLength(100).WithMessage("BillingAddress.City must not exceed 100 characters.");

            RuleFor(x => x.OrderDto.BillingAddress.State)
                .NotEmpty().WithMessage("BillingAddress.State is required.")
                .MaximumLength(100).WithMessage("BillingAddress.State must not exceed 100 characters.");

            RuleFor(x => x.OrderDto.BillingAddress.PostalCode)
                .NotEmpty().WithMessage("BillingAddress.PostalCode is required.")
                .MaximumLength(20).WithMessage("BillingAddress.PostalCode must not exceed 20 characters.");

            RuleFor(x => x.OrderDto.BillingAddress.Country)
                .NotEmpty().WithMessage("BillingAddress.Country is required.")
                .MaximumLength(100).WithMessage("BillingAddress.Country must not exceed 100 characters.");
        });

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
