using Application.SupportAssist.Commands;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Validators;
using Domain.SupportAssistAggregate;
using FluentValidation.TestHelper;

namespace UnitTests.Application.SupportAssist.Validators;

public class CreateSupportTicketValidatorTests
{
    private readonly CreateSupportTicketValidator _validator = new();

    private static CreateSupportTicketCommand CreateValidCommand() => new()
    {
        TicketDto = new CreateSupportTicketDto
        {
            OrderId = "order-1",
            Subject = "Valid Subject",
            Description = "Valid Description",
            Category = TicketCategory.PaymentIssue,
            Status = TicketStatus.Open
        }
    };

    [Fact]
    public void Validate_WhenOrderIdEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.OrderId = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.OrderId);
    }

    [Fact]
    public void Validate_WhenSubjectEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.Subject = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Subject);
    }

    [Fact]
    public void Validate_WhenSubjectExceeds200Characters_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.Subject = new string('a', 201);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Subject);
    }

    [Fact]
    public void Validate_WhenDescriptionEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.Description = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds1000Characters_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.Description = new string('a', 1001);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Description);
    }

    [Fact]
    public void Validate_WhenCategoryInvalid_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.Category = (TicketCategory)999;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Category);
    }

    [Fact]
    public void Validate_WhenStatusInvalid_HasError()
    {
        var command = CreateValidCommand();
        command.TicketDto.Status = (TicketStatus)999;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TicketDto.Status);
    }

    [Fact]
    public void Validate_WhenValidCommand_NoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(TicketCategory.OrderIssue)]
    [InlineData(TicketCategory.PaymentIssue)]
    [InlineData(TicketCategory.ProductIssue)]
    [InlineData(TicketCategory.Other)]
    public void Validate_AllValidCategories_NoErrors(TicketCategory category)
    {
        var command = CreateValidCommand();
        command.TicketDto.Category = category;

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.TicketDto.Category);
    }

    [Theory]
    [InlineData(TicketStatus.Open)]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.Closed)]
    public void Validate_AllValidStatuses_NoErrors(TicketStatus status)
    {
        var command = CreateValidCommand();
        command.TicketDto.Status = status;

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.TicketDto.Status);
    }
}
