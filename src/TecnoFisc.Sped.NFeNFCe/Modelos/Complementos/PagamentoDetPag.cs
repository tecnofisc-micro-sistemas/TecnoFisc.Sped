// Scaffolding revisÃƒÂ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record PagamentoDetPag
{
    /// <summary>XML: <c>indPag</c>.</summary>
    public string? IndPag { get; init; }

    /// <summary>XML: <c>tPag</c>.</summary>
    public string? TPag { get; init; }

    /// <summary>XML: <c>xPag</c>.</summary>
    public string? XPag { get; init; }

    /// <summary>XML: <c>vPag</c>.</summary>
    public decimal? VPag { get; init; }

    /// <summary>XML: <c>dPag</c>.</summary>
    public DateOnly? DPag { get; init; }

    /// <summary>XML: <c>CNPJPag</c>.</summary>
    public string? CNPJPag { get; init; }

    /// <summary>XML: <c>UFPag</c>.</summary>
    public string? UFPag { get; init; }

    /// <summary>XML: <c>card</c>.</summary>
    public PagamentoDetPagCard? Card { get; init; }
}
