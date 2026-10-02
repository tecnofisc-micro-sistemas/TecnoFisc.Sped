// Scaffolding revisÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record DeclaracaoImportacao
{
    /// <summary>XML: <c>nDI</c>.</summary>
    public string? NDI { get; init; }

    /// <summary>XML: <c>dDI</c>.</summary>
    public DateOnly? DDI { get; init; }

    /// <summary>XML: <c>xLocDesemb</c>.</summary>
    public string? XLocDesemb { get; init; }

    /// <summary>XML: <c>UFDesemb</c>.</summary>
    public string? UFDesemb { get; init; }

    /// <summary>XML: <c>dDesemb</c>.</summary>
    public DateOnly? DDesemb { get; init; }

    /// <summary>XML: <c>tpViaTransp</c>.</summary>
    public string? TpViaTransp { get; init; }

    /// <summary>XML: <c>vAFRMM</c>.</summary>
    public decimal? VAFRMM { get; init; }

    /// <summary>XML: <c>tpIntermedio</c>.</summary>
    public string? TpIntermedio { get; init; }

    /// <summary>XML: <c>CNPJ</c>.</summary>
    public string? CNPJ { get; init; }

    /// <summary>XML: <c>CPF</c>.</summary>
    public string? CPF { get; init; }

    /// <summary>XML: <c>UFTerceiro</c>.</summary>
    public string? UFTerceiro { get; init; }

    /// <summary>XML: <c>cExportador</c>.</summary>
    public string? CExportador { get; init; }

    /// <summary>XML: <c>adi</c>.</summary>
    public IReadOnlyList<DeclaracaoImportacaoAdi> Adi { get; init; } = [];
}
