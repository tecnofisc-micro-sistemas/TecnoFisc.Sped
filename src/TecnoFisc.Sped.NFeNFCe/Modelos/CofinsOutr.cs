namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>COFINSOutr</c> — COFINS em outras operações (CST 49–75, 98, 99).
/// Contém <c>CST</c>, um <c>xs:choice</c> interno entre forma percentual (<c>vBC</c>/<c>pCOFINS</c>)
/// e forma específica (<c>qBCProd</c>/<c>vAliqProd</c>), modelado como campos nullable,
/// mais o campo obrigatório <c>vCOFINS</c>.
/// </summary>
public sealed record CofinsOutr : Cofins
{
    // -------------------------------------------------------------------------
    // Forma percentual (choice interno — par vBC/pCOFINS; mutuamente exclusivo com qBCProd/vAliqProd)
    // -------------------------------------------------------------------------

    /// <summary><c>vBC</c> — valor da base de cálculo do COFINS (forma percentual, opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pCOFINS</c> — alíquota do COFINS em percentual (forma percentual, opcional).</summary>
    public decimal? PCOFINS { get; init; }

    // -------------------------------------------------------------------------
    // Forma específica (choice interno — par qBCProd/vAliqProd)
    // -------------------------------------------------------------------------

    /// <summary><c>qBCProd</c> — quantidade vendida (forma específica, opcional).</summary>
    public decimal? QBCProd { get; init; }

    /// <summary><c>vAliqProd</c> — alíquota do COFINS em reais por unidade (forma específica, opcional).</summary>
    public decimal? VAliqProd { get; init; }

    // -------------------------------------------------------------------------
    // Campo obrigatório
    // -------------------------------------------------------------------------

    /// <summary><c>vCOFINS</c> — valor do COFINS apurado.</summary>
    public required decimal VCOFINS { get; init; }
}
