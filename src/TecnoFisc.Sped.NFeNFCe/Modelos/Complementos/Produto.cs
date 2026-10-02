// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Produto
{
    /// <summary>XML: <c>cBarra</c>.</summary>
    public string? CBarra { get; init; }

    /// <summary>XML: <c>NVE</c>.</summary>
    public IReadOnlyList<string> NVE { get; init; } = [];

    /// <summary>XML: <c>indEscala</c>.</summary>
    public string? IndEscala { get; init; }

    /// <summary>XML: <c>CNPJFab</c>.</summary>
    public string? CNPJFab { get; init; }

    /// <summary>XML: <c>cBenef</c>.</summary>
    public string? CBenef { get; init; }

    /// <summary>XML: <c>gCred</c>.</summary>
    public IReadOnlyList<ProdutoGCred> GCred { get; init; } = [];

    /// <summary>XML: <c>tpCredPresIBSZFM</c>.</summary>
    public string? TpCredPresIBSZFM { get; init; }

    /// <summary>XML: <c>cBarraTrib</c>.</summary>
    public string? CBarraTrib { get; init; }

    /// <summary>XML: <c>vFrete</c>.</summary>
    public decimal? VFrete { get; init; }

    /// <summary>XML: <c>vSeg</c>.</summary>
    public decimal? VSeg { get; init; }

    /// <summary>XML: <c>vDesc</c>.</summary>
    public decimal? VDesc { get; init; }

    /// <summary>XML: <c>vOutro</c>.</summary>
    public decimal? VOutro { get; init; }

    /// <summary>XML: <c>indBemMovelUsado</c>.</summary>
    public string? IndBemMovelUsado { get; init; }

    /// <summary>XML: <c>DI</c>.</summary>
    public IReadOnlyList<DeclaracaoImportacao> DI { get; init; } = [];

    /// <summary>XML: <c>detExport</c>.</summary>
    public IReadOnlyList<DetalheExportacao> DetExport { get; init; } = [];

    /// <summary>XML: <c>xPed</c>.</summary>
    public string? XPed { get; init; }

    /// <summary>XML: <c>nItemPed</c>.</summary>
    public string? NItemPed { get; init; }

    /// <summary>XML: <c>nFCI</c>.</summary>
    public string? NFCI { get; init; }

    /// <summary>XML: <c>rastro</c>.</summary>
    public IReadOnlyList<Rastreabilidade> Rastro { get; init; } = [];

    /// <summary>XML: <c>infProdNFF</c>.</summary>
    public ProdutoInfProdNFF? InfProdNFF { get; init; }

    /// <summary>XML: <c>infProdEmb</c>.</summary>
    public ProdutoInfProdEmb? InfProdEmb { get; init; }

    /// <summary>XML: <c>veicProd</c>.</summary>
    public VeiculoProduto? VeicProd { get; init; }

    /// <summary>XML: <c>med</c>.</summary>
    public Medicamento? Med { get; init; }

    /// <summary>XML: <c>arma</c>.</summary>
    public IReadOnlyList<Armamento> Arma { get; init; } = [];

    /// <summary>XML: <c>comb</c>.</summary>
    public Combustivel? Comb { get; init; }

    /// <summary>XML: <c>nRECOPI</c>.</summary>
    public string? NRECOPI { get; init; }
}
