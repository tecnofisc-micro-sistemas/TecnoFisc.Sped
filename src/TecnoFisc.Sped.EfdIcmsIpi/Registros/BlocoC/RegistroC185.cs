using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C185 — Informações Complementares das Operações de Saída de
/// Mercadorias Sujeitas à Substituição Tributária (cód. 01, 1B, 04, 55 e 65).
/// IND_OPER do registro pai C100 deve ser "1" (Saída). Não pode coexistir com C186 no mesmo C170.
/// Nível hierárquico 3, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 96.
/// </summary>
[RegistroSped(Codigo = "C185", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC185 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C185";

    /// <summary>Número sequencial do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public int? NumItem { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela 4.3.1.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação, conforme Ajuste SINIEF 07/01.</summary>
    [CampoSped(Ordem = 5, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Código do motivo da restituição ou complementação conforme Tabela 5.7.</summary>
    [CampoSped(Ordem = 6, Tamanho = 5)]
    public string? CodMotRestCompl { get; set; }

    /// <summary>Quantidade do item convertida na unidade de controle de estoque informada no Registro 0200.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 6)]
    public decimal? QuantConv { get; set; }

    /// <summary>Unidade adotada para informar o campo QUANT_CONV, conforme Registro 0190.</summary>
    [CampoSped(Ordem = 8, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Valor unitário da mercadoria considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitConv { get; set; }

    /// <summary>Valor unitário para o ICMS na operação, caso não houvesse ST, considerando a unidade do campo QUANT_CONV e a redução da base de cálculo do ICMS ST na tributação, se houver.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsNaOperacaoConv { get; set; }

    /// <summary>Valor unitário do ICMS OP calculado conforme a legislação de cada UF, considerando a unidade utilizada para informar QUANT_CONV, utilizado para cálculo de ressarcimento/restituição de ST.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpConv { get; set; }

    /// <summary>Valor médio unitário do ICMS OP das mercadorias em estoque, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpEstoqueConv { get; set; }

    /// <summary>Valor médio unitário do ICMS ST, incluindo FCP ST, das mercadorias em estoque, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStEstoqueConv { get; set; }

    /// <summary>Valor médio unitário do FCP ST agregado ao ICMS das mercadorias em estoque, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpIcmsStEstoqueConv { get; set; }

    /// <summary>Valor unitário do total do ICMS ST, incluindo FCP ST, a ser restituído/ressarcido, calculado conforme a legislação de cada UF, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvRest { get; set; }

    /// <summary>Valor unitário correspondente à parcela de ICMS FCP ST que compõe o campo VL_UNIT_ICMS_ST_CONV_REST, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvRest { get; set; }

    /// <summary>Valor unitário do complemento do ICMS ST, incluindo FCP ST, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvCompl { get; set; }

    /// <summary>Valor unitário correspondente à parcela de ICMS FCP ST que compõe o campo VL_UNIT_ICMS_ST_CONV_COMPL, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvCompl { get; set; }
}
