// Scaffolding revisÃƒÂ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIssqn
{
    /// <summary>XML: <c>vServ</c>.</summary>
    public decimal? VServ { get; init; }

    /// <summary>XML: <c>vBC</c>.</summary>
    public decimal? VBC { get; init; }

    /// <summary>XML: <c>vISS</c>.</summary>
    public decimal? VISS { get; init; }

    /// <summary>XML: <c>vPIS</c>.</summary>
    public decimal? VPIS { get; init; }

    /// <summary>XML: <c>vCOFINS</c>.</summary>
    public decimal? VCOFINS { get; init; }

    /// <summary>XML: <c>dCompet</c>.</summary>
    public DateOnly? DCompet { get; init; }

    /// <summary>XML: <c>vDeducao</c>.</summary>
    public decimal? VDeducao { get; init; }

    /// <summary>XML: <c>vOutro</c>.</summary>
    public decimal? VOutro { get; init; }

    /// <summary>XML: <c>vDescIncond</c>.</summary>
    public decimal? VDescIncond { get; init; }

    /// <summary>XML: <c>vDescCond</c>.</summary>
    public decimal? VDescCond { get; init; }

    /// <summary>XML: <c>vISSRet</c>.</summary>
    public decimal? VISSRet { get; init; }

    /// <summary>XML: <c>cRegTrib</c>.</summary>
    public string? CRegTrib { get; init; }
}
