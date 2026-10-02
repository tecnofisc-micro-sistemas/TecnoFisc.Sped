// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CombustivelOrigComb
{
    /// <summary>XML: <c>indImport</c>.</summary>
    public string? IndImport { get; init; }

    /// <summary>XML: <c>cUFOrig</c>.</summary>
    public string? CUFOrig { get; init; }

    /// <summary>XML: <c>pOrig</c>.</summary>
    public decimal? POrig { get; init; }
}
