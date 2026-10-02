// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMono
{
    /// <summary>XML: <c>gIBSMonoAdRem</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRem? GIBSMonoAdRem { get; init; }

    /// <summary>XML: <c>gIBSMonoAdValorem</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValorem? GIBSMonoAdValorem { get; init; }

    /// <summary>XML: <c>gCBSMonoAdRem</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRem? GCBSMonoAdRem { get; init; }

    /// <summary>XML: <c>gCBSMonoAdValorem</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValorem? GCBSMonoAdValorem { get; init; }

    /// <summary>XML: <c>vTotIBSMonoItem</c>.</summary>
    public decimal? VTotIBSMonoItem { get; init; }

    /// <summary>XML: <c>vTotCBSMonoItem</c>.</summary>
    public decimal? VTotCBSMonoItem { get; init; }
}
