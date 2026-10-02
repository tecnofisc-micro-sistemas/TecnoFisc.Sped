// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeInfPAA
{
    /// <summary>XML: <c>CNPJPAA</c>.</summary>
    public string? CNPJPAA { get; init; }

    /// <summary>XML: <c>PAASignature</c>.</summary>
    public InfNFeInfPAAPAASignature? PAASignature { get; init; }
}
