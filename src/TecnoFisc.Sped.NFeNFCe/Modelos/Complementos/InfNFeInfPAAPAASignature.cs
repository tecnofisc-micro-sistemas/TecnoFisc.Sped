// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeInfPAAPAASignature
{
    /// <summary>XML: <c>SignatureValue</c>.</summary>
    public string? SignatureValue { get; init; }

    /// <summary>XML: <c>RSAKeyValue</c>.</summary>
    public InfNFeInfPAAPAASignatureRSAKeyValue? RSAKeyValue { get; init; }
}
