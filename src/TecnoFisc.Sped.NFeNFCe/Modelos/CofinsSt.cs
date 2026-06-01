namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>COFINSST</c> — COFINS substituição tributária de um item.
/// Elemento distinto de <c>COFINS</c>: não possui <c>CST</c> e não deriva de <see cref="Cofins"/>.
/// Contém um <c>xs:choice</c> interno entre forma percentual (<c>vBC</c>/<c>pCOFINS</c>)
/// e forma específica (<c>qBCProd</c>/<c>vAliqProd</c>), modelado como campos nullable,
/// mais o obrigatório <c>vCOFINS</c> e o opcional <c>indSomaCOFINSST</c>.
/// </summary>
public sealed record CofinsSt
{
    // -------------------------------------------------------------------------
    // Forma percentual (choice interno — par vBC/pCOFINS)
    // -------------------------------------------------------------------------

    /// <summary><c>vBC</c> — valor da base de cálculo do COFINS ST (forma percentual, opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pCOFINS</c> — alíquota do COFINS ST em percentual (forma percentual, opcional).</summary>
    public decimal? PCOFINS { get; init; }

    // -------------------------------------------------------------------------
    // Forma específica (choice interno — par qBCProd/vAliqProd)
    // -------------------------------------------------------------------------

    /// <summary><c>qBCProd</c> — quantidade vendida (forma específica, opcional).</summary>
    public decimal? QBCProd { get; init; }

    /// <summary><c>vAliqProd</c> — alíquota do COFINS ST em reais por unidade (forma específica, opcional).</summary>
    public decimal? VAliqProd { get; init; }

    // -------------------------------------------------------------------------
    // Campos fixos
    // -------------------------------------------------------------------------

    /// <summary><c>vCOFINS</c> — valor do COFINS ST.</summary>
    public required decimal VCOFINS { get; init; }

    /// <summary>
    /// <c>indSomaCOFINSST</c> — indica se o valor do COFINSST compõe o valor total da NF-e
    /// (0 = não soma; 1 = soma). Opcional (minOccurs="0").
    /// </summary>
    public int? IndSomaCOFINSST { get; init; }
}
