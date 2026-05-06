namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza do evento de sucessão — campo IND_NAT_EVEN do Registro F800.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 282.
/// </summary>
public enum IndicadorNaturezaEvento
{
    /// <summary>01 — Incorporação.</summary>
    Incorporacao = 1,

    /// <summary>02 — Fusão.</summary>
    Fusao = 2,

    /// <summary>03 — Cisão Total.</summary>
    CisaoTotal = 3,

    /// <summary>04 — Cisão Parcial.</summary>
    CisaoParcial = 4,

    /// <summary>99 — Outros.</summary>
    Outros = 99,
}
