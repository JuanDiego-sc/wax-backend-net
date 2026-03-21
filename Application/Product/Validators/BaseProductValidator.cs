using System;
using FluentValidation;

namespace Application.Product.Validators;

public class BaseProductValidator<T, TDto> : AbstractValidator<T>
    where TDto : class
{
 //TODO: Implement common validation rules for product commands and queries here
}
