using Application.Interfaces.DTOs;

namespace Application.Interfaces.Services;

public interface IDimensionParser
{
    bool TryParse(string raw, out ParsedDimensions dimensions);
}