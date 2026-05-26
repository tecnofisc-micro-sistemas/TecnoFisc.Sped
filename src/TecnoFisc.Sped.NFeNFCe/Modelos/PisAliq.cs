namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>PISAliq</c> — PIS tributado por alíquota percentual ad valorem
/// (CST 01 – alíquota normal; CST 02 – alíquota diferenciada).
/// Campos: <c>CST</c>, <c>vBC</c>, <c>pPIS</c>, <c>vPIS</c>.
/// </summary>
public sealed record PisAliq : Pis
{
    /// <summary><c>vBC</c> — valor da base de cálculo do PIS.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>pPIS</c> — alíquota do PIS em percentual.</summary>
    public required decimal PPIS { get; init; }

    /// <summary><c>vPIS</c> — valor do PIS apurado.</summary>
    public required decimal VPIS { get; init; }
}
