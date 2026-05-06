namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de origem das deduções diversas — campo IND_ORI_DED do Registro F700.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 281.
/// </summary>
public enum IndicadorOrigemDeducoesDiversas
{
    /// <summary>01 — Créditos Presumidos - Medicamentos.</summary>
    CreditosPresumidosMedicamentos = 1,

    /// <summary>02 — Créditos Admitidos no Regime Cumulativo – Bebidas Frias.</summary>
    CreditosAdmitidosRegimeCumulativoBebidas = 2,

    /// <summary>03 — Contribuição Paga pelo Substituto Tributário - ZFM.</summary>
    ContribuicaoPagaSubstitutoTributarioZfm = 3,

    /// <summary>04 — Substituição Tributária – Não Ocorrência do Fato Gerador Presumido.</summary>
    SubstituicaoTributariaNaoOcorrenciaFatoGerador = 4,

    /// <summary>99 — Outras Deduções.</summary>
    OutrasDeducoes = 99,
}
