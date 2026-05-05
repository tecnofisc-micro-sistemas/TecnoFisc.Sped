namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de operação de serviço — campo IND_OPER do Registro A100.
/// Valores conforme Guia Prático v1.35, p. 96.
/// </summary>
public enum IndicadorOperacaoServico
{
    /// <summary>0 — Serviço Contratado pelo Estabelecimento.</summary>
    ServicoContratado = 0,

    /// <summary>1 — Serviço Prestado pelo Estabelecimento.</summary>
    ServicoPrestado = 1,
}
