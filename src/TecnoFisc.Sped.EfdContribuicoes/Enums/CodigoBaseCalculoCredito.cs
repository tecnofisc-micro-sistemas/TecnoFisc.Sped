namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código de Base de Cálculo do Crédito — Tabela 4.3.7 do Guia Prático EFD Contribuições.
/// Codifica a base de cálculo dos créditos apurados no período, no preenchimento de
/// registros de documentos e operações geradoras de crédito (Blocos A, C, D, F e 1).
/// Campo COD_BC_CRED.
/// </summary>
public enum CodigoBaseCalculoCredito
{
    /// <summary>01 — Aquisição de bens para revenda.</summary>
    AquisicaoBensRevenda = 1,

    /// <summary>02 — Aquisição de bens utilizados como insumo.</summary>
    AquisicaoBensInsumo = 2,

    /// <summary>03 — Aquisição de serviços utilizados como insumo.</summary>
    AquisicaoServicosInsumo = 3,

    /// <summary>04 — Energia elétrica e térmica, inclusive sob a forma de vapor.</summary>
    EnergiaEletricaTermica = 4,

    /// <summary>05 — Aluguéis de prédios.</summary>
    AlugueisPredios = 5,

    /// <summary>06 — Aluguéis de máquinas e equipamentos.</summary>
    AlugueisMaquinasEquipamentos = 6,

    /// <summary>07 — Armazenagem de mercadoria e frete na operação de venda.</summary>
    ArmazenagemFreteVenda = 7,

    /// <summary>08 — Contraprestações de arrendamento mercantil.</summary>
    ArrendamentoMercantil = 8,

    /// <summary>09 — Máquinas, equipamentos e outros bens incorporados ao ativo imobilizado (crédito sobre encargos de depreciação).</summary>
    AtivoImobilizadoEncargosDepreciacao = 9,

    /// <summary>10 — Máquinas, equipamentos e outros bens incorporados ao ativo imobilizado (crédito com base no valor de aquisição).</summary>
    AtivoImobilizadoValorAquisicao = 10,

    /// <summary>11 — Amortização e Depreciação de edificações e benfeitorias em imóveis.</summary>
    AmortizacaoDepreciacaoEdificacoes = 11,

    /// <summary>12 — Devolução de Vendas Sujeitas à Incidência Não-Cumulativa.</summary>
    DevolucaoVendasNaoCumulativa = 12,

    /// <summary>13 — Outras Operações com Direito a Crédito.</summary>
    OutrasOperacoesComCredito = 13,

    /// <summary>14 — Atividade de Transporte de Cargas – Subcontratação.</summary>
    TransporteCargasSubcontratacao = 14,

    /// <summary>15 — Atividade Imobiliária – Custo Incorrido de Unidade Imobiliária.</summary>
    AtividadeImobiliariaCustoIncorrido = 15,

    /// <summary>16 — Atividade Imobiliária – Custo Orçado de unidade não concluída.</summary>
    AtividadeImobiliariaCustoOrcado = 16,

    /// <summary>17 — Atividade de Prestação de Serviços de Limpeza, Conservação e Manutenção – vale-transporte, vale-refeição ou vale-alimentação, fardamento ou uniforme.</summary>
    PrestacaoServicosLimpezaConservacao = 17,

    /// <summary>18 — Estoque de abertura de bens.</summary>
    EstoqueAberturaBens = 18,
}
