using System.Globalization;
using System.Text.RegularExpressions;
using Application.Interfaces.DTOs;
using Application.Interfaces.Services;

namespace Infrastructure.Quotation;

public partial class DimensionParser : IDimensionParser
{
    private static readonly Regex Pattern = DimensionParserRegex();

    public bool TryParse(string raw, out ParsedDimensions dimensions)
    {
        dimensions = new ParsedDimensions(0, 0, null);
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var match = Pattern.Match(raw);
        if (!match.Success) return false;

        var width = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var height = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        decimal? depth = match.Groups[3].Success
            ? decimal.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture)
            : null;

        dimensions = new ParsedDimensions(width, height, depth);
        return true;
    }

    [GeneratedRegex(@"^\s*(\d+)\s*x\s*(\d+)\s*(?:x\s*(\d+))?\s*cm\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "es-EC")]
    private static partial Regex DimensionParserRegex();
}