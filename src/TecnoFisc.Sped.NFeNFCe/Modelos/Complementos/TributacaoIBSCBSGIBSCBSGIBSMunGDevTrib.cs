// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGIBSMunGDevTrib
{
    /// <summary>XML: <c>pDevTrib</c>.</summary>
    public decimal? PDevTrib { get; init; }

    /// <summary>XML: <c>vDevTrib</c>.</summary>
    public decimal? VDevTrib { get; init; }
}
