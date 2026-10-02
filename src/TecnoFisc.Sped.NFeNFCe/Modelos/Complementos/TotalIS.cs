// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIS
{
    /// <summary>XML: <c>vIS</c>.</summary>
    public decimal? VIS { get; init; }
}
