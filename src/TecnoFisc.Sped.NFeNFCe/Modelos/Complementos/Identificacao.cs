// Scaffolding revisÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Identificacao
{
    /// <summary>XML: <c>dPrevEntrega</c>.</summary>
    public DateOnly? DPrevEntrega { get; init; }

    /// <summary>XML: <c>cMunFGIBS</c>.</summary>
    public string? CMunFGIBS { get; init; }

    /// <summary>XML: <c>tpNFDebito</c>.</summary>
    public string? TpNFDebito { get; init; }

    /// <summary>XML: <c>tpNFCredito</c>.</summary>
    public string? TpNFCredito { get; init; }

    /// <summary>XML: <c>cIndOp</c>.</summary>
    public string? CIndOp { get; init; }

    /// <summary>XML: <c>dhCont</c>.</summary>
    public DateTimeOffset? DhCont { get; init; }

    /// <summary>XML: <c>xJust</c>.</summary>
    public string? XJust { get; init; }

    /// <summary>XML: <c>NFref</c>.</summary>
    public IReadOnlyList<ReferenciaNota> NFref { get; init; } = [];

    /// <summary>XML: <c>gCompraGov</c>.</summary>
    public IdentificacaoGCompraGov? GCompraGov { get; init; }

    /// <summary>XML: <c>gPagAntecipado</c>.</summary>
    public IdentificacaoGPagAntecipado? GPagAntecipado { get; init; }
}
