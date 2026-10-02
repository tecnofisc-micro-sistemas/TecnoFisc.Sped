// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record LocalEntrega
{
    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>CPF</c>.</summary>
    public string? CPF { get; init; }

    /// <summary>XML: <c>xNome</c>.</summary>
    public string? XNome { get; init; }

    /// <summary>XML: <c>xLgr</c>.</summary>
    public string? XLgr { get; init; }

    /// <summary>XML: <c>nro</c>.</summary>
    public string? Nro { get; init; }

    /// <summary>XML: <c>xCpl</c>.</summary>
    public string? XCpl { get; init; }

    /// <summary>XML: <c>xBairro</c>.</summary>
    public string? XBairro { get; init; }

    /// <summary>XML: <c>cMun</c>.</summary>
    public string? CMun { get; init; }

    /// <summary>XML: <c>xMun</c>.</summary>
    public string? XMun { get; init; }

    /// <summary>XML: <c>UF</c>.</summary>
    public string? UF { get; init; }

    /// <summary>XML: <c>CEP</c>.</summary>
    public string? CEP { get; init; }

    /// <summary>XML: <c>cPais</c>.</summary>
    public string? CPais { get; init; }

    /// <summary>XML: <c>xPais</c>.</summary>
    public string? XPais { get; init; }

    /// <summary>XML: <c>fone</c>.</summary>
    public string? Fone { get; init; }

    /// <summary>XML: <c>email</c>.</summary>
    public string? Email { get; init; }

    /// <summary>XML: <c>IE</c>.</summary>
    public string? IE { get; init; }
}
