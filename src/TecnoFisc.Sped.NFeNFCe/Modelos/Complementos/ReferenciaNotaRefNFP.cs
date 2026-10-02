// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ReferenciaNotaRefNFP
{
    /// <summary>XML: <c>cUF</c>.</summary>
    public string? CUF { get; init; }

    /// <summary>XML: <c>AAMM</c>.</summary>
    public string? AAMM { get; init; }

    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>CPF</c>.</summary>
    public string? CPF { get; init; }

    /// <summary>XML: <c>IE</c>.</summary>
    public string? IE { get; init; }

    /// <summary>XML: <c>mod</c>.</summary>
    public string? Mod { get; init; }

    /// <summary>XML: <c>serie</c>.</summary>
    public string? Serie { get; init; }

    /// <summary>XML: <c>nNF</c>.</summary>
    public string? NNF { get; init; }
}
