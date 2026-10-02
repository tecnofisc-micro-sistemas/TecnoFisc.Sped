// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGTribRegular
{
    /// <summary>XML: <c>CSTReg</c>.</summary>
    public string? CSTReg { get; init; }

    /// <summary>XML: <c>cClassTribReg</c>.</summary>
    public string? CClassTribReg { get; init; }

    /// <summary>XML: <c>pAliqEfetRegIBSUF</c>.</summary>
    public decimal? PAliqEfetRegIBSUF { get; init; }

    /// <summary>XML: <c>vTribRegIBSUF</c>.</summary>
    public decimal? VTribRegIBSUF { get; init; }

    /// <summary>XML: <c>pAliqEfetRegIBSMun</c>.</summary>
    public decimal? PAliqEfetRegIBSMun { get; init; }

    /// <summary>XML: <c>vTribRegIBSMun</c>.</summary>
    public decimal? VTribRegIBSMun { get; init; }

    /// <summary>XML: <c>pAliqEfetRegCBS</c>.</summary>
    public decimal? PAliqEfetRegCBS { get; init; }

    /// <summary>XML: <c>vTribRegCBS</c>.</summary>
    public decimal? VTribRegCBS { get; init; }
}
