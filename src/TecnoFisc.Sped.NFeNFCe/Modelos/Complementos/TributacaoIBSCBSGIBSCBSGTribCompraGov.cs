// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGTribCompraGov
{
    /// <summary>XML: <c>pAliqIBSUF</c>.</summary>
    public decimal? PAliqIBSUF { get; init; }

    /// <summary>XML: <c>vTribIBSUF</c>.</summary>
    public decimal? VTribIBSUF { get; init; }

    /// <summary>XML: <c>pAliqIBSMun</c>.</summary>
    public decimal? PAliqIBSMun { get; init; }

    /// <summary>XML: <c>vTribIBSMun</c>.</summary>
    public decimal? VTribIBSMun { get; init; }

    /// <summary>XML: <c>pAliqCBS</c>.</summary>
    public decimal? PAliqCBS { get; init; }

    /// <summary>XML: <c>vTribCBS</c>.</summary>
    public decimal? VTribCBS { get; init; }
}
