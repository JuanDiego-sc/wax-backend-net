using Application.User.Commands;
using FluentValidation;

namespace Application.User.Validators;

public class CreateOrUpdateBillingAddressCommandValidator 
    : AbstractValidator<CreateOrUpdateBillingAddressCommand>
{
    public CreateOrUpdateBillingAddressCommandValidator()
    {
        RuleFor(x => x.BillingInfo.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.BillingInfo.Line1)
            .NotEmpty().WithMessage("Line1 is required.")
            .MaximumLength(200);

        RuleFor(x => x.BillingInfo.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.BillingInfo.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100);

        RuleFor(x => x.BillingInfo.PostalCode)
            .NotEmpty().WithMessage("PostalCode is required.")
            .MaximumLength(20);

        RuleFor(x => x.BillingInfo.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100);

        RuleFor(x => x.BillingInfo.IdentificationNumber)
            .NotEmpty().WithMessage("IdentificationNumber is required.")
            .MaximumLength(20).WithMessage("IdentificationNumber must not exceed 20 characters.");
        
        RuleFor(x => x.BillingInfo.IdentificationType)
            .NotEmpty().WithMessage("IdentificationType is required.")
            .MaximumLength(20).WithMessage("IdentificationType must not exceed 20 characters.");
        
        RuleFor(x => x.BillingInfo.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100);

        RuleFor(x => x.BillingInfo.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100);

        RuleFor(x => x.BillingInfo.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20);
    }
}