using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C880 — Informações Complementares das Operações de Saída de Mercadorias
/// Sujeitas à Substituição Tributária (CF-E-SAT) (cód. 59).
/// A obrigatoriedade e a forma de escrituração são definidas pela UF de domicílio do contribuinte.
/// Nível hierárquico 4, ocorrência 1:1 por registro C870.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 159.
/// </summary>
[RegistroSped(Codigo = "C880", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC880 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C880";

    /// <summary>Código do motivo da restituição ou complementação conforme Tabela 5.7.</summary>
    /// <remarks>
    /// <b>V017 (Guide 3.1.0 item 6):</b> nova validação no campo 02 <c>COD_MOT_REST_COMPL</c>.
    /// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
    /// </remarks>
    [CampoSped(Ordem = 2, Tamanho = 5)]
    public string? CodMotRestCompl { get; set; }

    /// <summary>Quantidade do item convertida na unidade de controle de estoque informada no Registro 0200, a critério de cada UF.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6)]
    public decimal? QuantConv { get; set; }

    /// <summary>Unidade adotada para informar o campo QUANT_CONV, conforme Registro 0190.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Valor unitário da mercadoria, considerando a unidade utilizada para informar o campo QUANT_CONV.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitConv { get; set; }

    /// <summary>Valor unitário para o ICMS na operação, caso não houvesse ST, considerando a unidade utilizada para informar QUANT_CONV e a redução da base de cálculo do ICMS ST na tributação, se houver.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsNaOperacaoConv { get; set; }

    /// <summary>Valor unitário do ICMS OP calculado conforme a legislação de cada UF, utilizado para cálculo de ressarcimento/restituição de ST, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsOpConv { get; set; }

    /// <summary>Valor médio unitário do ICMS OP das mercadorias em estoque, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsOpEstoqueConv { get; set; }

    /// <summary>Valor médio unitário do ICMS ST, incluindo FCP ST, das mercadorias em estoque, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsStEstoqueConv { get; set; }

    /// <summary>Valor médio unitário do FCP ST agregado ao ICMS das mercadorias em estoque, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitFcpIcmsStEstoqueConv { get; set; }

    /// <summary>Valor unitário do total do ICMS ST, incluindo FCP ST, a ser restituído/ressarcido, calculado conforme a legislação de cada UF, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsStConvRest { get; set; }

    /// <summary>Valor unitário correspondente à parcela de ICMS FCP ST que compõe o campo VL_UNIT_ICMS_ST_CONV_REST, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitFcpStConvRest { get; set; }

    /// <summary>Valor unitário do complemento do ICMS ST, incluindo FCP ST, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsStConvCompl { get; set; }

    /// <summary>Valor unitário correspondente à parcela de ICMS FCP ST que compõe o campo VL_UNIT_ICMS_ST_CONV_COMPL, considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitFcpStConvCompl { get; set; }
}
