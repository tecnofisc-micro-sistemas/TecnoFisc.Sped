// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record RetencoesTributos
{
    /// <summary>XML: <c>vRetPIS</c>.</summary>
    public decimal? VRetPIS { get; init; }

    /// <summary>XML: <c>vRetCOFINS</c>.</summary>
    public decimal? VRetCOFINS { get; init; }

    /// <summary>XML: <c>vRetCSLL</c>.</summary>
    public decimal? VRetCSLL { get; init; }

    /// <summary>XML: <c>vBCIRRF</c>.</summary>
    public decimal? VBCIRRF { get; init; }

    /// <summary>XML: <c>vIRRF</c>.</summary>
    public decimal? VIRRF { get; init; }

    /// <summary>XML: <c>vBCRetPrev</c>.</summary>
    public decimal? VBCRetPrev { get; init; }

    /// <summary>XML: <c>vRetPrev</c>.</summary>
    public decimal? VRetPrev { get; init; }
}
