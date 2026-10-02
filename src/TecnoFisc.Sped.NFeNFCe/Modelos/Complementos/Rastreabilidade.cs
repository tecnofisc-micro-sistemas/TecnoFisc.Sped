// Scaffolding revisÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Rastreabilidade
{
    /// <summary>XML: <c>nLote</c>.</summary>
    public string? NLote { get; init; }

    /// <summary>XML: <c>qLote</c>.</summary>
    public decimal? QLote { get; init; }

    /// <summary>XML: <c>dFab</c>.</summary>
    public DateOnly? DFab { get; init; }

    /// <summary>XML: <c>dVal</c>.</summary>
    public DateOnly? DVal { get; init; }

    /// <summary>XML: <c>cAgreg</c>.</summary>
    public string? CAgreg { get; init; }
}
