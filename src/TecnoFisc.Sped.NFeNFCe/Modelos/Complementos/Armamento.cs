// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Armamento
{
    /// <summary>XML: <c>tpArma</c>.</summary>
    public string? TpArma { get; init; }

    /// <summary>XML: <c>nSerie</c>.</summary>
    public string? NSerie { get; init; }

    /// <summary>XML: <c>nCano</c>.</summary>
    public string? NCano { get; init; }

    /// <summary>XML: <c>descr</c>.</summary>
    public string? Descr { get; init; }
}
