// Scaffolding revisÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeAvulsa
{
    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>xOrgao</c>.</summary>
    public string? XOrgao { get; init; }

    /// <summary>XML: <c>matr</c>.</summary>
    public string? Matr { get; init; }

    /// <summary>XML: <c>xAgente</c>.</summary>
    public string? XAgente { get; init; }

    /// <summary>XML: <c>fone</c>.</summary>
    public string? Fone { get; init; }

    /// <summary>XML: <c>UF</c>.</summary>
    public string? UF { get; init; }

    /// <summary>XML: <c>nDAR</c>.</summary>
    public string? NDAR { get; init; }

    /// <summary>XML: <c>dEmi</c>.</summary>
    public DateOnly? DEmi { get; init; }

    /// <summary>XML: <c>vDAR</c>.</summary>
    public decimal? VDAR { get; init; }

    /// <summary>XML: <c>repEmi</c>.</summary>
    public string? RepEmi { get; init; }

    /// <summary>XML: <c>dPag</c>.</summary>
    public DateOnly? DPag { get; init; }
}
