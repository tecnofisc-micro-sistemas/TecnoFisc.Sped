// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValorem
{
    /// <summary>XML: <c>gMonoPadrao</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadrao? GMonoPadrao { get; init; }

    /// <summary>XML: <c>gMonoReten</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoReten? GMonoReten { get; init; }

    /// <summary>XML: <c>gMonoRet</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoRet? GMonoRet { get; init; }

    /// <summary>XML: <c>gpBioDiferenca</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferenca? GpBioDiferenca { get; init; }
}
