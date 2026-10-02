// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TransporteTransporta
{
    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>CPF</c>.</summary>
    public string? CPF { get; init; }

    /// <summary>XML: <c>xNome</c>.</summary>
    public string? XNome { get; init; }

    /// <summary>XML: <c>IE</c>.</summary>
    public string? IE { get; init; }

    /// <summary>XML: <c>xEnder</c>.</summary>
    public string? XEnder { get; init; }

    /// <summary>XML: <c>xMun</c>.</summary>
    public string? XMun { get; init; }

    /// <summary>XML: <c>UF</c>.</summary>
    public string? UF { get; init; }
}
