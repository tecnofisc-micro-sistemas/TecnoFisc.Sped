namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do período de apuração do IPI — campo IND_APUR do Registro C170.
/// Valores válidos pelo Guia Prático v1.35: <c>0, 1</c>.
/// </summary>
public enum IndicadorApuracaoIpi
{
    /// <summary>0 — Apuração mensal.</summary>
    Mensal = 0,

    /// <summary>1 — Apuração decendial.</summary>
    Decendial = 1,
}
