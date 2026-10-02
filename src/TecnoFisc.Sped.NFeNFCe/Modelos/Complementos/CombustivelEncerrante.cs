// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CombustivelEncerrante
{
    /// <summary>XML: <c>nBico</c>.</summary>
    public string? NBico { get; init; }

    /// <summary>XML: <c>nBomba</c>.</summary>
    public string? NBomba { get; init; }

    /// <summary>XML: <c>nTanque</c>.</summary>
    public string? NTanque { get; init; }

    /// <summary>XML: <c>vEncIni</c>.</summary>
    public decimal? VEncIni { get; init; }

    /// <summary>XML: <c>vEncFin</c>.</summary>
    public decimal? VEncFin { get; init; }
}
