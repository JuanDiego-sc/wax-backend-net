using Application.SupportAssist.Commands;
using Application.SupportAssist.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.SupportAssist.Validators;

public class DeleteSupportTicketValidatorTests
{
    private readonly DeleteSupportTicketValidator _validator = new();

    [Fact]
    public void Validate_WhenTicketIdEmpty_HasError()
    {
        var command = new DeleteSupportTicketCommand { TicketId = "" };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketId);
    }

    [Fact]
    public void Validate_WhenTicketIdNull_HasError()
    {
        var command = new DeleteSupportTicketCommand { TicketId = null! };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketId);
    }

    [Fact]
    public void Validate_WhenTicketIdValid_NoErrors()
    {
        var command = new DeleteSupportTicketCommand { TicketId = "ticket-123" };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
