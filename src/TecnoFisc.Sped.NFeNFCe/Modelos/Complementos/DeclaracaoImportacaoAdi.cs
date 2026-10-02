// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record DeclaracaoImportacaoAdi
{
    /// <summary>XML: <c>nAdicao</c>.</summary>
    public string? NAdicao { get; init; }

    /// <summary>XML: <c>nSeqAdic</c>.</summary>
    public string? NSeqAdic { get; init; }

    /// <summary>XML: <c>cFabricante</c>.</summary>
    public string? CFabricante { get; init; }

    /// <summary>XML: <c>vDescDI</c>.</summary>
    public decimal? VDescDI { get; init; }

    /// <summary>XML: <c>nDraw</c>.</summary>
    public string? NDraw { get; init; }
}
