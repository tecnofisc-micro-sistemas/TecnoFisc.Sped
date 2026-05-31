using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C181 — Informações Complementares das Operações de Devolução de Saída de
/// Mercadorias Sujeitas à Substituição Tributária (cód. 01, 1B, 04 e 55).
/// IND_OPER do registro pai C100 deve ser "0" (Entrada). Não pode coexistir com C180 no mesmo C170.
/// Nível hierárquico 4, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 93.
/// </summary>
[RegistroSped(Codigo = "C181", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC181 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C181";

    /// <summary>Código do motivo da restituição ou complementação conforme Tabela 5.7.</summary>
    /// <remarks>
    /// <b>V017 (Guide 3.1.0 item 6):</b> nova validação no campo 02 <c>COD_MOT_REST_COMPL</c>.
    /// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
    /// </remarks>
    [CampoSped(Ordem = 2, Tamanho = 5, Obrigatorio = true)]
    public string? CodMotRestCompl { get; set; }

    /// <summary>Quantidade do item convertida na unidade de controle de estoque informada no Registro 0200.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? QuantConv { get; set; }

    /// <summary>Unidade adotada para informar o campo QUANT_CONV, conforme Registro 0190.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Código do modelo do documento fiscal de saída, conforme Tabela 4.1.1. Valores válidos: 01, 1B, 02, 2D, 04, 55, 59, 60, 65.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodModSaida { get; set; }

    /// <summary>Número de série do documento de saída em papel.</summary>
    [CampoSped(Ordem = 6, Tamanho = 3)]
    public string? SerieSaida { get; set; }

    /// <summary>Número de série de fabricação do equipamento ECF.</summary>
    [CampoSped(Ordem = 7, Tamanho = 21)]
    public string? EcfFabSaida { get; set; }

    /// <summary>Número do documento fiscal de saída. Obrigatório quando COD_MOD_SAIDA for 01, 1B, 02, 2D ou 04.</summary>
    [CampoSped(Ordem = 8, Tamanho = 9)]
    public int? NumDocSaida { get; set; }

    /// <summary>Chave do documento fiscal eletrônico de saída. Obrigatório quando COD_MOD_SAIDA for 55, 59, 60 ou 65.</summary>
    [CampoSped(Ordem = 9, Tamanho = 44)]
    public ChaveAcesso? ChvDfeSaida { get; set; }

    /// <summary>Data da emissão do documento fiscal de saída (formato DDMMAAAA).</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtDocSaida { get; set; }

    /// <summary>Número do item em que foi escriturada a saída num registro C185, C380, C480 ou C815 quando o contribuinte informar a saída em um arquivo de perfil A.</summary>
    [CampoSped(Ordem = 11, Tamanho = 3)]
    public int? NumItemSaida { get; set; }

    /// <summary>Valor unitário da mercadoria considerando a unidade utilizada para informar QUANT_CONV, correspondente ao valor do campo VL_UNIT_CONV preenchido na ocasião da saída.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitConvSaida { get; set; }

    /// <summary>Valor médio unitário do ICMS OP das mercadorias em estoque, correspondente ao valor do campo VL_UNIT_ICMS_OP_ESTOQUE_CONV preenchido na ocasião da saída.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpEstoqueConvSaida { get; set; }

    /// <summary>Valor médio unitário do ICMS ST, incluindo FCP ST, das mercadorias em estoque, correspondente ao valor do campo VL_UNIT_ICMS_ST_ESTOQUE_CONV preenchido na ocasião da saída.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStEstoqueConvSaida { get; set; }

    /// <summary>Valor médio unitário do FCP ST agregado ao ICMS das mercadorias em estoque, correspondente ao valor do campo VL_UNIT_FCP_ICMS_ST_ESTOQUE_CONV preenchido na ocasião da saída.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpIcmsStEstoqueConvSaida { get; set; }

    /// <summary>Valor unitário para o ICMS na operação, correspondente ao valor do campo VL_UNIT_ICMS_NA_OPERACAO_CONV preenchido na ocasião da saída.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsNaOperacaoConvSaida { get; set; }

    /// <summary>Valor unitário do ICMS correspondente ao valor do campo VL_UNIT_ICMS_OP_CONV preenchido na ocasião da saída.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpConvSaida { get; set; }

    /// <summary>Valor unitário do total do ICMS ST, incluindo FCP ST, a ser restituído/ressarcido, correspondente ao estorno do complemento apurado na operação de saída.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvRest { get; set; }

    /// <summary>Valor unitário correspondente à parcela de ICMS FCP ST que compõe o campo VL_UNIT_ICMS_ST_CONV_REST, considerando a unidade utilizada para informar o campo QUANT_CONV.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvRest { get; set; }

    /// <summary>Valor unitário do estorno do ressarcimento/restituição, incluindo FCP ST, apurado na operação de saída.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvCompl { get; set; }

    /// <summary>Valor unitário correspondente à parcela de ICMS FCP ST que compõe o campo VL_UNIT_ICMS_ST_CONV_COMPL, considerando a unidade utilizada para informar o campo QUANT_CONV.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvCompl { get; set; }
}
