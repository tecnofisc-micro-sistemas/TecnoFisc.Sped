// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRem
{
    /// <summary>XML: <c>gMonoPadrao</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadrao? GMonoPadrao { get; init; }

    /// <summary>XML: <c>gMonoReten</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoReten? GMonoReten { get; init; }

    /// <summary>XML: <c>gMonoRet</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoRet? GMonoRet { get; init; }

    /// <summary>XML: <c>gpBioDiferenca</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGpBioDiferenca? GpBioDiferenca { get; init; }
}
