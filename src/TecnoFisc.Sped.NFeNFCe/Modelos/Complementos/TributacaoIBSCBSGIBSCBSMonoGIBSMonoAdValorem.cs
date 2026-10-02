// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValorem
{
    /// <summary>XML: <c>gMonoPadrao</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadrao? GMonoPadrao { get; init; }

    /// <summary>XML: <c>gMonoReten</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoReten? GMonoReten { get; init; }

    /// <summary>XML: <c>gMonoRet</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRet? GMonoRet { get; init; }

    /// <summary>XML: <c>gpBioDiferenca</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGpBioDiferenca? GpBioDiferenca { get; init; }
}
