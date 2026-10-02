// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Intermediador
{
    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>idCadIntTran</c>.</summary>
    public string? IdCadIntTran { get; init; }
}
