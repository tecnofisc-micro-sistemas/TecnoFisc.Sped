namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de operação escriturada — campo IND_OPER do Registro F100.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 233.
/// </summary>
public enum IndicadorOperacaoF100
{
    /// <summary>
    /// 0 – Operação Representativa de Aquisição, Custos, Despesa ou Encargos, ou Receitas,
    /// Sujeita à Incidência de Crédito de PIS/Pasep ou Cofins (CST 50 a 66).
    /// </summary>
    AquisicaoCustosEncargosComCredito = 0,

    /// <summary>
    /// 1 – Operação Representativa de Receita Auferida Sujeita ao Pagamento da Contribuição
    /// para o PIS/Pasep e da Cofins (CST 01, 02, 03 ou 05).
    /// </summary>
    ReceitaAuferidaSujeitaContribuicao = 1,

    /// <summary>
    /// 2 – Operação Representativa de Receita Auferida Não Sujeita ao Pagamento da Contribuição
    /// para o PIS/Pasep e da Cofins (CST 04, 06, 07, 08, 09, 49 ou 99).
    /// </summary>
    ReceitaAuferidaNaoSujeitaContribuicao = 2,
}
