// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record IdentificacaoGCompraGov
{
    /// <summary>XML: <c>tpEnteGov</c>.</summary>
    public string? TpEnteGov { get; init; }

    /// <summary>XML: <c>pRedutor</c>.</summary>
    public decimal? PRedutor { get; init; }

    /// <summary>XML: <c>tpOperGov</c>.</summary>
    public string? TpOperGov { get; init; }

    /// <summary>XML: <c>refDFeAnt</c>.</summary>
    public IReadOnlyList<string> RefDFeAnt { get; init; } = [];
}
