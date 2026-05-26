namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>PISST</c> — PIS substituição tributária de um item.
/// Elemento distinto de <c>PIS</c>: não possui <c>CST</c> e não deriva de <see cref="Pis"/>.
/// Contém um <c>xs:choice</c> interno entre forma percentual (<c>vBC</c>/<c>pPIS</c>)
/// e forma específica (<c>qBCProd</c>/<c>vAliqProd</c>), modelado como campos nullable,
/// mais o obrigatório <c>vPIS</c> e o opcional <c>indSomaPISST</c>.
/// </summary>
public sealed record PisSt
{
    // -------------------------------------------------------------------------
    // Forma percentual (choice interno — par vBC/pPIS)
    // -------------------------------------------------------------------------

    /// <summary><c>vBC</c> — valor da base de cálculo do PIS ST (forma percentual, opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pPIS</c> — alíquota do PIS ST em percentual (forma percentual, opcional).</summary>
    public decimal? PPIS { get; init; }

    // -------------------------------------------------------------------------
    // Forma específica (choice interno — par qBCProd/vAliqProd)
    // -------------------------------------------------------------------------

    /// <summary><c>qBCProd</c> — quantidade vendida (forma específica, opcional).</summary>
    public decimal? QBCProd { get; init; }

    /// <summary><c>vAliqProd</c> — alíquota do PIS ST em reais por unidade (forma específica, opcional).</summary>
    public decimal? VAliqProd { get; init; }

    // -------------------------------------------------------------------------
    // Campos fixos
    // -------------------------------------------------------------------------

    /// <summary><c>vPIS</c> — valor do PIS ST.</summary>
    public required decimal VPIS { get; init; }

    /// <summary>
    /// <c>indSomaPISST</c> — indica se o valor do PISST compõe o valor total da NF-e
    /// (0 = não soma; 1 = soma). Opcional (minOccurs="0").
    /// </summary>
    public int? IndSomaPISST { get; init; }
}
