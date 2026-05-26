namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>PISQtde</c> — PIS tributado por alíquota específica (quantidade × valor por unidade)
/// (CST 03 — base de cálculo = quantidade vendida × alíquota por unidade de produto).
/// Campos: <c>CST</c>, <c>qBCProd</c>, <c>vAliqProd</c>, <c>vPIS</c>.
/// </summary>
public sealed record PisQtde : Pis
{
    /// <summary><c>qBCProd</c> — quantidade vendida (base de cálculo em unidades).</summary>
    public required decimal QBCProd { get; init; }

    /// <summary><c>vAliqProd</c> — alíquota do PIS em reais por unidade de produto.</summary>
    public required decimal VAliqProd { get; init; }

    /// <summary><c>vPIS</c> — valor do PIS apurado.</summary>
    public required decimal VPIS { get; init; }
}
