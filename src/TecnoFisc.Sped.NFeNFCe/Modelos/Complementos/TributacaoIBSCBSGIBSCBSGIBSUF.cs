// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGIBSUF
{
    /// <summary>XML: <c>pIBSUF</c>.</summary>
    public decimal? PIBSUF { get; init; }

    /// <summary>XML: <c>gDif</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSUFGDif? GDif { get; init; }

    /// <summary>XML: <c>gDevTrib</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSUFGDevTrib? GDevTrib { get; init; }

    /// <summary>XML: <c>gRed</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSUFGRed? GRed { get; init; }

    /// <summary>XML: <c>vIBSUF</c>.</summary>
    public decimal? VIBSUF { get; init; }
}
