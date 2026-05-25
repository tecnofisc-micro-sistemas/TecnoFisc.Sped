namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Código do Plano de Contas Referencial usado para o mapeamento das contas analíticas —
/// campo COD_PLAN_REF do Registro 0000 da ECD. Preenchido apenas quando a pessoa jurídica
/// realiza o mapeamento para os planos referenciais na ECD.
/// </summary>
public enum CodigoPlanoContasReferencial
{
    /// <summary>1 — PJ em Geral – Lucro Real.</summary>
    PessoaJuridicaGeralLucroReal = 1,

    /// <summary>2 — PJ em Geral – Lucro Presumido.</summary>
    PessoaJuridicaGeralLucroPresumido = 2,

    /// <summary>3 — Financeiras – Lucro Real.</summary>
    FinanceirasLucroReal = 3,

    /// <summary>4 — Seguradoras – Lucro Real.</summary>
    SeguradorasLucroReal = 4,

    /// <summary>5 — Imunes e Isentas em Geral.</summary>
    ImunesIsentasGeral = 5,

    /// <summary>6 — Imunes e Isentas – Financeiras.</summary>
    ImunesIsentasFinanceiras = 6,

    /// <summary>7 — Imunes e Isentas – Seguradoras.</summary>
    ImunesIsentasSeguradoras = 7,

    /// <summary>8 — Entidades Fechadas de Previdência Complementar.</summary>
    EntidadesFechadasPrevidenciaComplementar = 8,

    /// <summary>9 — Partidos Políticos.</summary>
    PartidosPoliticos = 9,

    /// <summary>10 — Financeiras – Lucro Presumido.</summary>
    FinanceirasLucroPresumido = 10,

    /// <summary>11 — TEF – Tributação Específica do Futebol.</summary>
    TributacaoEspecificaFutebol = 11,
}
