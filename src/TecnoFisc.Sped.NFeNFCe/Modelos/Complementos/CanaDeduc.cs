// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CanaDeduc
{
    /// <summary>XML: <c>xDed</c>.</summary>
    public string? XDed { get; init; }

    /// <summary>XML: <c>vDed</c>.</summary>
    public decimal? VDed { get; init; }
}
