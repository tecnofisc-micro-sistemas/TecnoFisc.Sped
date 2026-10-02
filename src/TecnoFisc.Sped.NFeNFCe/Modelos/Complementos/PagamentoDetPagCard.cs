// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record PagamentoDetPagCard
{
    /// <summary>XML: <c>tpIntegra</c>.</summary>
    public string? TpIntegra { get; init; }

    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>tBand</c>.</summary>
    public string? TBand { get; init; }

    /// <summary>XML: <c>cAut</c>.</summary>
    public string? CAut { get; init; }

    /// <summary>XML: <c>CNPJReceb</c>.</summary>
    public string? CNPJReceb { get; init; }

    /// <summary>XML: <c>idTermPag</c>.</summary>
    public string? IdTermPag { get; init; }
}
