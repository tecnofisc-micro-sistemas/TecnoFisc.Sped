// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InformacoesAdicionaisProcRef
{
    /// <summary>XML: <c>nProc</c>.</summary>
    public string? NProc { get; init; }

    /// <summary>XML: <c>indProc</c>.</summary>
    public string? IndProc { get; init; }

    /// <summary>XML: <c>tpAto</c>.</summary>
    public string? TpAto { get; init; }
}
