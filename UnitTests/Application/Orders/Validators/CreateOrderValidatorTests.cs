using Application.Orders.Commands;
using Application.Orders.DTOs;
using Application.Orders.Validators;
using Domain.Entities;
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
            BillingAddress = new Address
            {
                Name = "Test User",
                Line1 = "123 Main St",
                City = "Test City",
                State = "TS",
                PostalCode = "12345",
                Country = "US"
            },
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
    public void Validate_WhenBillingAddressNull_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress = null!;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress);
    }

    [Fact]
    public void Validate_WhenBillingNameEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.Name = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.Name);
    }

    [Fact]
    public void Validate_WhenBillingNameExceeds100_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.Name = new string('a', 101);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.Name);
    }

    [Fact]
    public void Validate_WhenBillingLine1Empty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.Line1 = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.Line1);
    }

    [Fact]
    public void Validate_WhenBillingCityEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.City = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.City);
    }

    [Fact]
    public void Validate_WhenBillingStateEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.State = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.State);
    }

    [Fact]
    public void Validate_WhenBillingPostalCodeEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.PostalCode = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.PostalCode);
    }

    [Fact]
    public void Validate_WhenBillingCountryEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.OrderDto.BillingAddress.Country = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderDto.BillingAddress.Country);
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
