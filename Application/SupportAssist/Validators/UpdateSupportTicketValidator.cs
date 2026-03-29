using FluentValidation;
using Application.SupportAssist.Commands;

namespace Application.SupportAssist.Validators;

public class UpdateSupportTicketValidator : AbstractValidator<UpdateSupportTicketCommand>
{
    public UpdateSupportTicketValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(x => x.TicketDto.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200).WithMessage("Subject must not exceed 200 characters.");

        RuleFor(x => x.TicketDto.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.TicketDto.Category)
            .IsInEnum().WithMessage("Category is not valid.");

        RuleFor(x => x.TicketDto.Status)
            .IsInEnum().WithMessage("Status is not valid.");
    }
}
