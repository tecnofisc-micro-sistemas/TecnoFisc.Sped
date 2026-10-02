// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGCredPresOper
{
    /// <summary>XML: <c>vBCCredPres</c>.</summary>
    public decimal? VBCCredPres { get; init; }

    /// <summary>XML: <c>cCredPres</c>.</summary>
    public string? CCredPres { get; init; }

    /// <summary>XML: <c>gIBSCredPres</c>.</summary>
    public TributacaoIBSCBSGCredPresOperGIBSCredPres? GIBSCredPres { get; init; }

    /// <summary>XML: <c>gCBSCredPres</c>.</summary>
    public TributacaoIBSCBSGCredPresOperGCBSCredPres? GCBSCredPres { get; init; }
}
