// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeInfPAAPAASignatureRSAKeyValue
{
    /// <summary>XML: <c>Modulus</c>.</summary>
    public string? Modulus { get; init; }

    /// <summary>XML: <c>Exponent</c>.</summary>
    public string? Exponent { get; init; }
}
