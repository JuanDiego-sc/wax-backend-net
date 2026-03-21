using System;
using FluentValidation;

namespace Application.Basket.Validators;

public class BaseBasketValidator<T, TDto> : AbstractValidator<T>
    where TDto : class
{
 //TODO: Implement common validation rules for basket commands and queries here
 
}
