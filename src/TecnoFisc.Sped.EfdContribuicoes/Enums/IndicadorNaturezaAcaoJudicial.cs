namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza da ação judicial — campo IND_NAT_ACAO do Registro 1010.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 375.
/// Códigos 12–19 identificam ações com exigibilidade de contribuição suspensa.
/// </summary>
public enum IndicadorNaturezaAcaoJudicial
{
    /// <summary>01 — Decisão judicial transitada em julgado, a favor da pessoa jurídica.</summary>
    DecisaoTransitadaEmJulgadoFavoravel = 1,

    /// <summary>02 — Decisão judicial não transitada em julgado, a favor da pessoa jurídica.</summary>
    DecisaoNaoTransitadaEmJulgadoFavoravel = 2,

    /// <summary>03 — Decisão judicial oriunda de liminar em mandado de segurança.</summary>
    LiminarMandadoSeguranca = 3,

    /// <summary>04 — Decisão judicial oriunda de liminar em medida cautelar.</summary>
    LiminarMedidaCautelar = 4,

    /// <summary>05 — Decisão judicial oriunda de antecipação de tutela.</summary>
    AntecipacaoDeTutela = 5,

    /// <summary>06 — Decisão judicial vinculada a depósito administrativo ou judicial em montante integral.</summary>
    DepositoAdministrativoOuJudicial = 6,

    /// <summary>07 — Medida judicial em que a pessoa jurídica não é o autor.</summary>
    MedidaJudicialPessoaJuridicaNaoAutora = 7,

    /// <summary>08 — Súmula vinculante aprovada pelo STF ou STJ.</summary>
    SumulaVinculante = 8,

    /// <summary>09 — Decisão judicial oriunda de liminar em mandado de segurança coletivo.</summary>
    LiminarMandadoSegurancaColetivo = 9,

    /// <summary>12 — Decisão judicial não transitada em julgado, a favor da pessoa jurídica — Exigibilidade suspensa.</summary>
    DecisaoNaoTransitadaExigibilidadeSuspensa = 12,

    /// <summary>13 — Decisão judicial oriunda de liminar em mandado de segurança — Exigibilidade suspensa.</summary>
    LiminarMandadoSegurancaExigibilidadeSuspensa = 13,

    /// <summary>14 — Decisão judicial oriunda de liminar em medida cautelar — Exigibilidade suspensa.</summary>
    LiminarMedidaCautelarExigibilidadeSuspensa = 14,

    /// <summary>15 — Decisão judicial oriunda de antecipação de tutela — Exigibilidade suspensa.</summary>
    AntecipacaoTutelaExigibilidadeSuspensa = 15,

    /// <summary>16 — Decisão judicial vinculada a depósito administrativo ou judicial — Exigibilidade suspensa.</summary>
    DepositoExigibilidadeSuspensa = 16,

    /// <summary>17 — Medida judicial em que a pessoa jurídica não é o autor — Exigibilidade suspensa.</summary>
    MedidaJudicialNaoAutoraExigibilidadeSuspensa = 17,

    /// <summary>19 — Decisão judicial oriunda de liminar em mandado de segurança coletivo — Exigibilidade suspensa.</summary>
    LiminarMandadoSegurancaColetivoExigibilidadeSuspensa = 19,

    /// <summary>99 — Outros.</summary>
    Outros = 99,
}
