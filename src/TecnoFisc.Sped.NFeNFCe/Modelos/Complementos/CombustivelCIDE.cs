// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CombustivelCIDE
{
    /// <summary>XML: <c>qBCProd</c>.</summary>
    public decimal? QBCProd { get; init; }

    /// <summary>XML: <c>vAliqProd</c>.</summary>
    public decimal? VAliqProd { get; init; }

    /// <summary>XML: <c>vCIDE</c>.</summary>
    public decimal? VCIDE { get; init; }
}
