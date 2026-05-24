using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C380 — Informações Complementares das Operações de Saída de Mercadorias Sujeitas à
/// Substituição Tributária (código 02).
/// Detalha por item de mercadoria (C370) os valores unitários de ICMS ST para fins de
/// ressarcimento ou complementação, conforme legislação da UF.
/// Nível hierárquico 4, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 114.
/// </summary>
[RegistroSped(Codigo = "C380", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC380 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C380";

    /// <summary>Código do motivo da restituição ou complementação conforme Tabela 5.7 (por UF).</summary>
    /// <remarks>
    /// <b>V017 (Guide 3.1.0 item 6):</b> nova validação no campo 02 <c>COD_MOT_REST_COMPL</c>.
    /// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
    /// </remarks>
    [CampoSped(Ordem = 2, Tamanho = 5)]
    public string? CodMotRestCompl { get; set; }

    /// <summary>Quantidade do item convertida na unidade de controle de estoque do Registro 0200.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6)]
    public decimal? QuantConv { get; set; }

    /// <summary>Unidade adotada para informar o campo QUANT_CONV.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Valor unitário líquido da mercadoria na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitConv { get; set; }

    /// <summary>Valor unitário para o ICMS na operação caso não houvesse ST, na unidade QUANT_CONV, com redução de BC se houver.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsNaOperacaoConv { get; set; }

    /// <summary>Valor unitário do ICMS OP calculado conforme legislação da UF, na unidade QUANT_CONV, usado no cálculo de ressarcimento/restituição ST.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpConv { get; set; }

    /// <summary>Valor médio unitário do ICMS OP das mercadorias em estoque, na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsOpEstoqueConv { get; set; }

    /// <summary>Valor médio unitário do ICMS ST (incluindo FCP ST) das mercadorias em estoque, na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStEstoqueConv { get; set; }

    /// <summary>Valor médio unitário da parcela de FCP ST agregada ao ICMS ST em estoque, na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpIcmsStEstoqueConv { get; set; }

    /// <summary>Valor unitário total do ICMS ST (incluindo FCP ST) a ser restituído/ressarcido, na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvRest { get; set; }

    /// <summary>Parcela de FCP ST que compõe VL_UNIT_ICMS_ST_CONV_REST, na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvRest { get; set; }

    /// <summary>Valor unitário do complemento do ICMS (incluindo FCP ST), na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitIcmsStConvCompl { get; set; }

    /// <summary>Parcela de FCP ST que compõe VL_UNIT_ICMS_ST_CONV_COMPL, na unidade QUANT_CONV.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConvCompl { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 15, Tamanho = 3)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação conforme legislação da UF.</summary>
    [CampoSped(Ordem = 16, Tamanho = 4)]
    public Cfop? Cfop { get; set; }
}
