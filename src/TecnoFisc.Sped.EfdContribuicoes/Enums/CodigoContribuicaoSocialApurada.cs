namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código de Contribuição Social Apurada — Tabela 4.3.5 do Guia Prático EFD Contribuições.
/// Codifica os tipos de contribuição apurada no período nos registros de apuração da
/// contribuição (Bloco M) ou de ajustes.
/// </summary>
public enum CodigoContribuicaoSocialApurada
{
    /// <summary>01 — Contribuição não-cumulativa apurada a alíquota básica.</summary>
    NaoCumulativaAliquotaBasica = 1,

    /// <summary>02 — Contribuição não-cumulativa apurada a alíquotas diferenciadas.</summary>
    NaoCumulativaAliquotasDiferenciadas = 2,

    /// <summary>03 — Contribuição não-cumulativa apurada a alíquota por unidade de medida de produto.</summary>
    NaoCumulativaAliquotaUnidadeMedida = 3,

    /// <summary>04 — Contribuição não-cumulativa apurada a alíquota básica – Atividade Imobiliária.</summary>
    NaoCumulativaAliquotaBasicaAtividadeImobiliaria = 4,

    /// <summary>31 — Contribuição apurada por substituição tributária.</summary>
    SubstituicaoTributaria = 31,

    /// <summary>32 — Contribuição apurada por substituição tributária – Vendas à Zona Franca de Manaus.</summary>
    SubstituicaoTributariaZonaFrancaManaus = 32,

    /// <summary>51 — Contribuição cumulativa apurada a alíquota básica.</summary>
    CumulativaAliquotaBasica = 51,

    /// <summary>52 — Contribuição cumulativa apurada a alíquotas diferenciadas.</summary>
    CumulativaAliquotasDiferenciadas = 52,

    /// <summary>53 — Contribuição cumulativa apurada a alíquota por unidade de medida de produto.</summary>
    CumulativaAliquotaUnidadeMedida = 53,

    /// <summary>54 — Contribuição cumulativa apurada a alíquota básica – Atividade Imobiliária.</summary>
    CumulativaAliquotaBasicaAtividadeImobiliaria = 54,

    /// <summary>71 — Contribuição apurada de SCP – Incidência Não Cumulativa.</summary>
    ScpIncidenciaNaoCumulativa = 71,

    /// <summary>72 — Contribuição apurada de SCP – Incidência Cumulativa.</summary>
    ScpIncidenciaCumulativa = 72,

    /// <summary>99 — Contribuição para o PIS/Pasep – Folha de Salários.</summary>
    PisPasepFolhaSalarios = 99,
}
