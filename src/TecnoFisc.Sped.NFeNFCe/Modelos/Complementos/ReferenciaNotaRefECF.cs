// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ReferenciaNotaRefECF
{
    /// <summary>XML: <c>mod</c>.</summary>
    public string? Mod { get; init; }

    /// <summary>XML: <c>nECF</c>.</summary>
    public string? NECF { get; init; }

    /// <summary>XML: <c>nCOO</c>.</summary>
    public string? NCOO { get; init; }
}
