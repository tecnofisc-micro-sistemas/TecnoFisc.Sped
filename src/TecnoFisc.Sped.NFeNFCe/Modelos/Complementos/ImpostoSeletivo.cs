// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ImpostoSeletivo
{
    /// <summary>XML: <c>CSTIS</c>.</summary>
    public string? CSTIS { get; init; }

    /// <summary>XML: <c>cClassTribIS</c>.</summary>
    public string? CClassTribIS { get; init; }

    /// <summary>XML: <c>vBCIS</c>.</summary>
    public decimal? VBCIS { get; init; }

    /// <summary>XML: <c>pIS</c>.</summary>
    public decimal? PIS { get; init; }

    /// <summary>XML: <c>adRemIS</c>.</summary>
    public decimal? AdRemIS { get; init; }

    /// <summary>XML: <c>uTrib</c>.</summary>
    public string? UTrib { get; init; }

    /// <summary>XML: <c>qTrib</c>.</summary>
    public decimal? QTrib { get; init; }

    /// <summary>XML: <c>vIS</c>.</summary>
    public decimal? VIS { get; init; }
}
