// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGCBSGALCZFMCBS
{
    /// <summary>XML: <c>tpALCZFMCBS</c>.</summary>
    public string? TpALCZFMCBS { get; init; }

    /// <summary>XML: <c>nProcSuframa</c>.</summary>
    public string? NProcSuframa { get; init; }

    /// <summary>XML: <c>pAliqEfetRegCBS</c>.</summary>
    public decimal? PAliqEfetRegCBS { get; init; }

    /// <summary>XML: <c>vTribRegCBS</c>.</summary>
    public decimal? VTribRegCBS { get; init; }
}
