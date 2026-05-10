using Application.Interfaces.DTOs;
using Application.Interfaces.Services;
using Domain.ProductAggregate;

namespace Infrastructure.Quotation;

public class QuotationService(IQuotationRulesCache rules, IDimensionParser parser) : IQuotationService
{
    public async Task<QuotationResult> QuoteAsync(CustomProductDesign design, CancellationToken cancellationToken = default)
    {
        var defaultDepth = await rules.GetAsync("DEFAULT_DEPTH_CM", 5m, cancellationToken);
        var baseCost = await rules.GetAsync("BASE_COST", 5000m, cancellationToken);
        var margin = await rules.GetAsync("MARGIN_MULTIPLIER", 1.6m, cancellationToken);

        var materialKey = $"MATERIAL_{Normalize(design.Material)}";
        var materialUnit = await rules.GetAsync(materialKey,
            await rules.GetAsync("MATERIAL_default", 2m, cancellationToken),
            cancellationToken);

        decimal width = 0, height = 0, depth = defaultDepth;
        if (parser.TryParse(design.Dimensions, out var parsed))
        {
            width = parsed.WidthCm;
            height = parsed.HeightCm;
            depth = parsed.DepthCm ?? await rules.GetAsync($"TYPE_{Normalize(design.Type)}_DEFAULT_DEPTH", defaultDepth, cancellationToken);
        }

        var volume = width * height * depth;
        var subtotal = volume * materialUnit + baseCost;
        var total = (long)Math.Ceiling(subtotal * margin);

        var breakdown = new Dictionary<string, decimal>
        {
            ["volume_cm3"] = volume,
            ["material_unit_cents_cm3"] = materialUnit,
            ["base_cost_cents"] = baseCost,
            ["margin"] = margin,
            ["subtotal_cents"] = subtotal,
            ["total_cents"] = total
        };

        return new QuotationResult(total, breakdown);
    }

    private static string Normalize(string input)
        => input.Trim().ToLowerInvariant().Replace(' ', '_');
}
