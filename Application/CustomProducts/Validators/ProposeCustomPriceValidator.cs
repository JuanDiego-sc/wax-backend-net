using FluentValidation;

namespace Application.CustomProducts.Validators;

public class AdminProposePriceValidator : AbstractValidator<Commands.AdminProposePriceCommand>
{
    public AdminProposePriceValidator()
    {
        RuleFor(x => x.CustomProductId).NotEmpty();
        RuleFor(x => x.ProposeCustomPrice.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a cero");
        RuleFor(x => x.ProposeCustomPrice.Comment).MaximumLength(500);
    }
}

public class CustomerProposePriceValidator : AbstractValidator<Commands.CustomerProposePriceCommand>
{
    public CustomerProposePriceValidator()
    {
        RuleFor(x => x.CustomProductId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.ProposeCustomPrice.Amount).GreaterThan(0);
        RuleFor(x => x.ProposeCustomPrice.Comment).MaximumLength(500);
    }
}