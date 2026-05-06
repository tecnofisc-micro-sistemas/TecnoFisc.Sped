namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza cumulativa de uma receita ou dedução — campos IND_NAT_REC (Registro F600)
/// e IND_NAT_DED (Registro F700). Valores conforme Guia Prático EFD Contribuições v1.35, p. 279, 281.
/// </summary>
public enum IndicadorNaturezaCumulativa
{
    /// <summary>0 — Natureza Não Cumulativa.</summary>
    NaoCumulativa = 0,

    /// <summary>1 — Natureza Cumulativa.</summary>
    Cumulativa = 1,
}
