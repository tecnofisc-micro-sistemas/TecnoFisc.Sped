namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>COFINSAliq</c> — COFINS tributado por alíquota percentual ad valorem
/// (CST 01 – alíquota normal; CST 02 – alíquota diferenciada).
/// Campos: <c>CST</c>, <c>vBC</c>, <c>pCOFINS</c>, <c>vCOFINS</c>.
/// </summary>
public sealed record CofinsAliq : Cofins
{
    /// <summary><c>vBC</c> — valor da base de cálculo do COFINS.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>pCOFINS</c> — alíquota do COFINS em percentual.</summary>
    public required decimal PCOFINS { get; init; }

    /// <summary><c>vCOFINS</c> — valor do COFINS apurado.</summary>
    public required decimal VCOFINS { get; init; }
}
