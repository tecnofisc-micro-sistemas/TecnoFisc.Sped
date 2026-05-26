namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>COFINSQtde</c> — COFINS tributado por alíquota específica (quantidade × valor por unidade)
/// (CST 03 — base de cálculo = quantidade vendida × alíquota por unidade de produto).
/// Campos: <c>CST</c>, <c>qBCProd</c>, <c>vAliqProd</c>, <c>vCOFINS</c>.
/// </summary>
public sealed record CofinsQtde : Cofins
{
    /// <summary><c>qBCProd</c> — quantidade vendida (base de cálculo em unidades).</summary>
    public required decimal QBCProd { get; init; }

    /// <summary><c>vAliqProd</c> — alíquota do COFINS em reais por unidade de produto.</summary>
    public required decimal VAliqProd { get; init; }

    /// <summary><c>vCOFINS</c> — valor do COFINS apurado.</summary>
    public required decimal VCOFINS { get; init; }
}
