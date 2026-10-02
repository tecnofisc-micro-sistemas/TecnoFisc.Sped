// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TransporteVolLacres
{
    /// <summary>XML: <c>nLacre</c>.</summary>
    public string? NLacre { get; init; }
}
