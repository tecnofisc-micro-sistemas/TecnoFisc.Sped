// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGTransfCred
{
    /// <summary>XML: <c>vIBS</c>.</summary>
    public decimal? VIBS { get; init; }

    /// <summary>XML: <c>vCBS</c>.</summary>
    public decimal? VCBS { get; init; }
}
