namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da origem do crédito apurado — campo IND_CRED_ORI do Registro M100.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 296.
/// </summary>
public enum IndicadorOrigemCreditoApurado
{
    /// <summary>0 — Operações próprias.</summary>
    OperacoesProprias = 0,

    /// <summary>1 — Evento de incorporação, cisão ou fusão.</summary>
    EventoSuccessao = 1,
}
