// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CobrancaFat
{
    /// <summary>XML: <c>nFat</c>.</summary>
    public string? NFat { get; init; }

    /// <summary>XML: <c>vOrig</c>.</summary>
    public decimal? VOrig { get; init; }

    /// <summary>XML: <c>vDesc</c>.</summary>
    public decimal? VDesc { get; init; }

    /// <summary>XML: <c>vLiq</c>.</summary>
    public decimal? VLiq { get; init; }
}
