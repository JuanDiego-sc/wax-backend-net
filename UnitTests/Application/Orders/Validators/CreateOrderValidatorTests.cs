using Application.Orders.Commands;
using Application.Orders.DTOs;
using Application.Orders.Validators;
using Domain.OrderAggregate;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Orders.Validators;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    private static CreateOrderCommand CreateValidCommand() => new()
    {
        BasketId = "basket-1",
        OrderDto = new CreateOrderDto
        {
            PaymentSummary = new PaymentSummary
            {
                Brand = "Visa",
                Last4 = 4242,
                ExpMonth = 12,
                ExpYear = DateTime.UtcNow.Year + 1
            }
        }
    };

    [Fact]
    public void Validate_WhenBasketIdEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.BasketId = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BasketId);
    }

    [Fact]
    public void Validate_WhenPaymentSummaryNull_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.PaymentSummary = null!;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.PaymentSummary);
    }

    [Fact]
    public void Validate_WhenPaymentBrandEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.PaymentSummary.Brand = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.PaymentSummary.Brand);
    }

    [Fact]
    public void Validate_WhenPaymentLast4Invalid_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.PaymentSummary.Last4 = 999;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.PaymentSummary.Last4);
    }

    [Fact]
    public void Validate_WhenPaymentExpMonthInvalid_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.PaymentSummary.ExpMonth = 13;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.PaymentSummary.ExpMonth);
    }

    [Fact]
    public void Validate_WhenPaymentExpYearInPast_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.PaymentSummary.ExpYear = DateTime.UtcNow.Year - 1;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.PaymentSummary.ExpYear);
    }

    [Fact]
    public void Validate_WhenValid_NoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
