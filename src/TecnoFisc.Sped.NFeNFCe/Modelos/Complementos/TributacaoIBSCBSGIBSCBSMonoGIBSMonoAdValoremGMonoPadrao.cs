// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadrao
{
    /// <summary>XML: <c>vBCMono</c>.</summary>
    public decimal? VBCMono { get; init; }

    /// <summary>XML: <c>pAliqMonoUF</c>.</summary>
    public decimal? PAliqMonoUF { get; init; }

    /// <summary>XML: <c>vIBSMonoUF</c>.</summary>
    public decimal? VIBSMonoUF { get; init; }

    /// <summary>XML: <c>pAliqMonoMun</c>.</summary>
    public decimal? PAliqMonoMun { get; init; }

    /// <summary>XML: <c>vIBSMonoMun</c>.</summary>
    public decimal? VIBSMonoMun { get; init; }

    /// <summary>XML: <c>vIBSMono</c>.</summary>
    public decimal? VIBSMono { get; init; }
}
