using Application.SupportAssist.Commands;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Validators;
using Domain.SupportAssistAggregate;
using FluentValidation.TestHelper;

namespace UnitTests.Application.SupportAssist.Validators;

public class UpdateSupportTicketValidatorTests
{
    private readonly UpdateSupportTicketValidator _validator = new();

    private static UpdateSupportTicketCommand CreateValidCommand() => new()
    {
        TicketId = "ticket-1",
        TicketDto = new UpdateSupportTicketDto
        {
            Subject = "Valid Subject",
            Description = "Valid Description",
            Category = TicketCategory.PaymentIssue,
            Status = TicketStatus.InProgress
        }
    };

    [Fact]
    public void Validate_WhenTicketIdEmpty_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = "Valid Subject",
                Description = "Valid Description",
                Category = TicketCategory.PaymentIssue,
                Status = TicketStatus.InProgress
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketId);
    }

    [Fact]
    public void Validate_WhenSubjectEmpty_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "ticket-1",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = "",
                Description = "Valid Description",
                Category = TicketCategory.PaymentIssue,
                Status = TicketStatus.InProgress
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Subject);
    }

    [Fact]
    public void Validate_WhenSubjectExceeds200_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "ticket-1",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = new string('a', 201),
                Description = "Valid Description",
                Category = TicketCategory.PaymentIssue,
                Status = TicketStatus.InProgress
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Subject);
    }

    [Fact]
    public void Validate_WhenDescriptionEmpty_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "ticket-1",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = "Valid Subject",
                Description = "",
                Category = TicketCategory.PaymentIssue,
                Status = TicketStatus.InProgress
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds1000_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "ticket-1",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = "Valid Subject",
                Description = new string('a', 1001),
                Category = TicketCategory.PaymentIssue,
                Status = TicketStatus.InProgress
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Description);
    }

    [Fact]
    public void Validate_WhenCategoryInvalid_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "ticket-1",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = "Valid Subject",
                Description = "Valid Description",
                Category = (TicketCategory)999,
                Status = TicketStatus.InProgress
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Category);
    }

    [Fact]
    public void Validate_WhenStatusInvalid_HasError()
    {
        var command = new UpdateSupportTicketCommand
        {
            TicketId = "ticket-1",
            TicketDto = new UpdateSupportTicketDto
            {
                Subject = "Valid Subject",
                Description = "Valid Description",
                Category = TicketCategory.PaymentIssue,
                Status = (TicketStatus)999
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Status);
    }

    [Fact]
    public void Validate_WhenValid_NoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
