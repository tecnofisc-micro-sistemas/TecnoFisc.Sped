// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Cobranca
{
    /// <summary>XML: <c>fat</c>.</summary>
    public CobrancaFat? Fat { get; init; }

    /// <summary>XML: <c>dup</c>.</summary>
    public IReadOnlyList<CobrancaDup> Dup { get; init; } = [];
}
