// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ResponsavelTecnico
{
    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>xContato</c>.</summary>
    public string? XContato { get; init; }

    /// <summary>XML: <c>email</c>.</summary>
    public string? Email { get; init; }

    /// <summary>XML: <c>fone</c>.</summary>
    public string? Fone { get; init; }

    /// <summary>XML: <c>idCSRT</c>.</summary>
    public string? IdCSRT { get; init; }

    /// <summary>XML: <c>hashCSRT</c>.</summary>
    public string? HashCSRT { get; init; }
}
