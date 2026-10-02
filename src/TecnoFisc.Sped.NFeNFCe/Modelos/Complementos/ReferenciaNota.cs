// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ReferenciaNota
{
    /// <summary>XML: <c>refNFe</c>.</summary>
    public string? RefNFe { get; init; }

    /// <summary>XML: <c>refNFeSig</c>.</summary>
    public string? RefNFeSig { get; init; }

    /// <summary>XML: <c>refNF</c>.</summary>
    public ReferenciaNotaRefNF? RefNF { get; init; }

    /// <summary>XML: <c>refNFP</c>.</summary>
    public ReferenciaNotaRefNFP? RefNFP { get; init; }

    /// <summary>XML: <c>refCTe</c>.</summary>
    public string? RefCTe { get; init; }

    /// <summary>XML: <c>refECF</c>.</summary>
    public ReferenciaNotaRefECF? RefECF { get; init; }
}
