using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.CustomProducts.Validators;

public class SubmitCustomProductValidator : AbstractValidator<Commands.SubmitCustomProductCommand>
{
    private static readonly Regex DimensionsPattern = new(@"^\s*\d+\s*x\s*\d+\s*(x\s*\d+)?\s*cm\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SubmitCustomProductValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.CustomProduct.TaskId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CustomProduct.GlbUrl).NotEmpty().Must(IsAbsoluteUrl).WithMessage("GlbUrl debe ser una URL absoluta");
        RuleFor(x => x.CustomProduct.RawDescription).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.CustomProduct.Design.Type).NotEmpty();
        RuleFor(x => x.CustomProduct.Design.Material).NotEmpty();
        RuleFor(x => x.CustomProduct.Design.Dimensions)
            .Must(d => DimensionsPattern.IsMatch(d ?? string.Empty))
            .WithMessage("Dimensiones deben tener formato 25x15cm o 25x15x5cm");
    }

    private static bool IsAbsoluteUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}