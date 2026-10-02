// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Pagamento
{
    /// <summary>XML: <c>detPag</c>.</summary>
    public IReadOnlyList<PagamentoDetPag> DetPag { get; init; } = [];

    /// <summary>XML: <c>vTroco</c>.</summary>
    public decimal? VTroco { get; init; }
}
