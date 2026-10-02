// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Combustivel
{
    /// <summary>XML: <c>cProdANP</c>.</summary>
    public string? CProdANP { get; init; }

    /// <summary>XML: <c>descANP</c>.</summary>
    public string? DescANP { get; init; }

    /// <summary>XML: <c>pGLP</c>.</summary>
    public decimal? PGLP { get; init; }

    /// <summary>XML: <c>pGNn</c>.</summary>
    public decimal? PGNn { get; init; }

    /// <summary>XML: <c>pGNi</c>.</summary>
    public decimal? PGNi { get; init; }

    /// <summary>XML: <c>vPart</c>.</summary>
    public decimal? VPart { get; init; }

    /// <summary>XML: <c>CODIF</c>.</summary>
    public string? CODIF { get; init; }

    /// <summary>XML: <c>qTemp</c>.</summary>
    public decimal? QTemp { get; init; }

    /// <summary>XML: <c>UFCons</c>.</summary>
    public string? UFCons { get; init; }

    /// <summary>XML: <c>CIDE</c>.</summary>
    public CombustivelCIDE? CIDE { get; init; }

    /// <summary>XML: <c>encerrante</c>.</summary>
    public CombustivelEncerrante? Encerrante { get; init; }

    /// <summary>XML: <c>pBio</c>.</summary>
    public decimal? PBio { get; init; }

    /// <summary>XML: <c>origComb</c>.</summary>
    public IReadOnlyList<CombustivelOrigComb> OrigComb { get; init; } = [];
}
