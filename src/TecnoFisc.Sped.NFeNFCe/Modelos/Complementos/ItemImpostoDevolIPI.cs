// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ItemImpostoDevolIPI
{
    /// <summary>XML: <c>vIPIDevol</c>.</summary>
    public decimal? VIPIDevol { get; init; }
}
