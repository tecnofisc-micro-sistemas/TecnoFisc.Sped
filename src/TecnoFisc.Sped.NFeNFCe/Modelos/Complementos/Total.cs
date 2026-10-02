// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Total
{
    /// <summary>XML: <c>ISSQNtot</c>.</summary>
    public TotalIssqn? ISSQNtot { get; init; }

    /// <summary>XML: <c>retTrib</c>.</summary>
    public RetencoesTributos? RetTrib { get; init; }

    /// <summary>XML: <c>ISTot</c>.</summary>
    public TotalIS? ISTot { get; init; }

    /// <summary>XML: <c>IBSCBSTot</c>.</summary>
    public TotalIBSCBS? IBSCBSTot { get; init; }

    /// <summary>XML: <c>vNFTot</c>.</summary>
    public decimal? VNFTot { get; init; }
}
