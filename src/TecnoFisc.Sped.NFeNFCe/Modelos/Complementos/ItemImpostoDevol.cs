// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ItemImpostoDevol
{
    /// <summary>XML: <c>pDevol</c>.</summary>
    public decimal? PDevol { get; init; }

    /// <summary>XML: <c>IPI</c>.</summary>
    public ItemImpostoDevolIPI? IPI { get; init; }
}
