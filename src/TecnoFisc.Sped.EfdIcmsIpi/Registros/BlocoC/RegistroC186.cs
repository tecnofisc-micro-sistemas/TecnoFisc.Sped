using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C186 — Informações Complementares das Operações de Devolução de
/// Entradas de Mercadorias Sujeitas à Substituição Tributária (cód. 01, 1B, 04 e 55).
/// IND_OPER do registro pai C100 deve ser "1" (Saída). Não pode coexistir com C185 no mesmo C170.
/// Nível hierárquico 3, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 100.
/// </summary>
[RegistroSped(Codigo = "C186", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC186 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C186";

    /// <summary>Número sequencial do item no documento fiscal de saída.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public int? NumItem { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS no documento fiscal de saída.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação no documento fiscal de saída.</summary>
    [CampoSped(Ordem = 5, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Código do motivo da restituição ou complementação conforme Tabela 5.7. O terceiro caractere deve ser igual a 4.</summary>
    [CampoSped(Ordem = 6, Tamanho = 5)]
    public string? CodMotRestCompl { get; set; }

    /// <summary>Quantidade do item devolvida na unidade de controle de estoque informada no Registro 0200.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 6)]
    public decimal? QuantConv { get; set; }

    /// <summary>Unidade adotada para informar o campo QUANT_CONV, conforme Registro 0190.</summary>
    [CampoSped(Ordem = 8, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Código do modelo do documento fiscal de entrada, conforme Tabela 4.1.1. Valores válidos: 01, 1B, 04, 55.</summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? CodModEntrada { get; set; }

    /// <summary>Número de série do documento de entrada em papel. Obrigatório quando COD_MOD_ENTRADA for diferente de 55.</summary>
    [CampoSped(Ordem = 10, Tamanho = 3)]
    public string? SerieEntrada { get; set; }

    /// <summary>Número do documento fiscal de entrada. Obrigatório quando COD_MOD_ENTRADA for diferente de 55.</summary>
    [CampoSped(Ordem = 11, Tamanho = 9)]
    public int? NumDocEntrada { get; set; }

    /// <summary>Chave do documento fiscal eletrônico de entrada. Obrigatório quando COD_MOD_ENTRADA for igual a 55.</summary>
    [CampoSped(Ordem = 12, Tamanho = 44)]
    public ChaveAcesso? ChvDfeEntrada { get; set; }

    /// <summary>Data da emissão do documento fiscal de entrada (formato DDMMAAAA).</summary>
    [CampoSped(Ordem = 13, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDocEntrada { get; set; }

    /// <summary>Número do item do documento fiscal de entrada.</summary>
    [CampoSped(Ordem = 14, Tamanho = 3)]
    public int? NumItemEntrada { get; set; }

    /// <summary>Valor unitário da mercadoria considerando a unidade utilizada para informar QUANT_CONV, correspondente ao valor do campo VL_UNIT_CONV preenchido na ocasião da entrada.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitConvEntrada { get; set; }

    /// <summary>Valor unitário do ICMS correspondente ao valor do campo VL_UNIT_ICMS_OP_CONV preenchido na ocasião da entrada.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpConvEntrada { get; set; }

    /// <summary>Valor unitário da base de cálculo do imposto pago ou retido anteriormente por substituição, correspondente ao valor do campo VL_UNIT_BC_ICMS_ST_CONV preenchido na ocasião da entrada.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitBcIcmsStConvEntrada { get; set; }

    /// <summary>Valor unitário do imposto pago ou retido anteriormente por substituição, inclusive FCP se devido, correspondente ao valor do campo VL_UNIT_ICMS_ST_CONV preenchido na ocasião da entrada.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvEntrada { get; set; }

    /// <summary>Valor unitário do FCP ST, correspondente ao valor do campo VL_UNIT_FCP_ST_CONV preenchido na ocasião da entrada.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvEntrada { get; set; }
}
