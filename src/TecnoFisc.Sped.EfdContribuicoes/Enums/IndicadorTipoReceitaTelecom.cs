namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de receita consolidada — campo IND_REC do Registro D600.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 221.
/// </summary>
public enum IndicadorTipoReceitaTelecom
{
    /// <summary>0 — Receita própria — serviços prestados.</summary>
    ReceitaPropriaServicosPrestados = 0,

    /// <summary>1 — Receita própria — cobrança de débitos.</summary>
    ReceitaPropriaCobrancaDebitos = 1,

    /// <summary>2 — Receita própria — venda de serviço pré-pago — faturamento de períodos anteriores.</summary>
    ReceitaPropriaPrePagoFaturamentoPeriodosAnteriores = 2,

    /// <summary>3 — Receita própria — venda de serviço pré-pago — faturamento no período.</summary>
    ReceitaPropriaPrePagoFaturamentoPeriodo = 3,

    /// <summary>4 — Outras receitas próprias de serviços de comunicação e telecomunicação.</summary>
    OutrasReceitasPropriasComunicacaoTelecom = 4,

    /// <summary>5 — Receita própria — co-faturamento.</summary>
    ReceitaPropriaCoFaturamento = 5,

    /// <summary>6 — Receita própria — serviços a faturar em período futuro.</summary>
    ReceitaPropriaServicosAFaturar = 6,

    /// <summary>7 — Outras receitas próprias de natureza não-cumulativa.</summary>
    OutrasReceitasPropriasnNaoCumulativa = 7,

    /// <summary>8 — Outras receitas de terceiros.</summary>
    OutrasReceitasTerceiros = 8,

    /// <summary>9 — Outras receitas.</summary>
    OutrasReceitas = 9,
}
