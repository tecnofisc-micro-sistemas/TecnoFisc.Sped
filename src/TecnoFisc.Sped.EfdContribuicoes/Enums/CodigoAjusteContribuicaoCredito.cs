namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código de Ajustes de Contribuição ou Créditos — Tabela 4.3.8 do Guia Prático EFD
/// Contribuições. Identifica a natureza do ajuste lançado em registros de ajuste de
/// contribuição/crédito (campo COD_AJ).
/// </summary>
public enum CodigoAjusteContribuicaoCredito
{
    /// <summary>01 — Ajuste Oriundo de Ação Judicial.</summary>
    AcaoJudicial = 1,

    /// <summary>02 — Ajuste Oriundo de Processo Administrativo.</summary>
    ProcessoAdministrativo = 2,

    /// <summary>03 — Ajuste Oriundo da Legislação Tributária.</summary>
    LegislacaoTributaria = 3,

    /// <summary>04 — Ajuste Oriundo Especificamente do RTT.</summary>
    Rtt = 4,

    /// <summary>05 — Ajuste Oriundo de Outras Situações.</summary>
    OutrasSituacoes = 5,

    /// <summary>06 — Estorno.</summary>
    Estorno = 6,

    /// <summary>07 — Ajuste da CPRB: Adoção do Regime de Caixa.</summary>
    CprbRegimeCaixa = 7,

    /// <summary>08 — Ajuste da CPRB: Diferimento de Valores a Recolher no Período.</summary>
    CprbDiferimentoValoresRecolher = 8,

    /// <summary>09 — Ajuste da CPRB: Adição de Valores Diferidos em Período(s) Anterior(es).</summary>
    CprbAdicaoValoresDiferidos = 9,
}
