// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRem
{
    /// <summary>XML: <c>gMonoPadrao</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadrao? GMonoPadrao { get; init; }

    /// <summary>XML: <c>gMonoReten</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoReten? GMonoReten { get; init; }

    /// <summary>XML: <c>gMonoRet</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoRet? GMonoRet { get; init; }

    /// <summary>XML: <c>gpBioDiferenca</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGpBioDiferenca? GpBioDiferenca { get; init; }
}
