namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da origem do crédito extemporâneo — campo ORIG_CRED do Registro 1220.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 393.
/// </summary>
public enum IndicadorOrigemCreditoExtemporaneo
{
    /// <summary>01 — Crédito decorrente de operações próprias.</summary>
    OperacoesProprias = 1,

    /// <summary>02 — Crédito transferido por pessoa jurídica sucedida.</summary>
    TransferidoPorSucedida = 2,
}
