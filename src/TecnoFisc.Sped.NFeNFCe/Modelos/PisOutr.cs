namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>PISOutr</c> — PIS em outras operações (CST 49–75, 98, 99).
/// Contém <c>CST</c>, um <c>xs:choice</c> interno entre forma percentual (<c>vBC</c>/<c>pPIS</c>)
/// e forma específica (<c>qBCProd</c>/<c>vAliqProd</c>), modelado como campos nullable,
/// mais o campo obrigatório <c>vPIS</c>.
/// </summary>
public sealed record PisOutr : Pis
{
    // -------------------------------------------------------------------------
    // Forma percentual (choice interno — par vBC/pPIS; mutuamente exclusivo com qBCProd/vAliqProd)
    // -------------------------------------------------------------------------

    /// <summary><c>vBC</c> — valor da base de cálculo do PIS (forma percentual, opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pPIS</c> — alíquota do PIS em percentual (forma percentual, opcional).</summary>
    public decimal? PPIS { get; init; }

    // -------------------------------------------------------------------------
    // Forma específica (choice interno — par qBCProd/vAliqProd)
    // -------------------------------------------------------------------------

    /// <summary><c>qBCProd</c> — quantidade vendida (forma específica, opcional).</summary>
    public decimal? QBCProd { get; init; }

    /// <summary><c>vAliqProd</c> — alíquota do PIS em reais por unidade (forma específica, opcional).</summary>
    public decimal? VAliqProd { get; init; }

    // -------------------------------------------------------------------------
    // Campo obrigatório
    // -------------------------------------------------------------------------

    /// <summary><c>vPIS</c> — valor do PIS apurado.</summary>
    public required decimal VPIS { get; init; }
}
