using FluentValidation;
using Application.SupportAssist.Commands;

namespace Application.SupportAssist.Validators;

public class DeleteSupportTicketValidator : AbstractValidator<DeleteSupportTicketCommand>
{
    public DeleteSupportTicketValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");
    }
}
