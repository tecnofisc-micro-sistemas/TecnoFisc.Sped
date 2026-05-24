using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C180 — Informações Complementares das Operações de Entrada de Mercadorias
/// Sujeitas à Substituição Tributária (cód. 01, 1B, 04 e 55).
/// IND_OPER do registro pai C100 deve ser "0" (Entrada). Não pode coexistir com C181 no mesmo C170.
/// Nível hierárquico 4, ocorrência 1:1 (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 91.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 item 23):</b> descrição do campo 11 <c>NUM_DA</c> reescrita.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "C180", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC180 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C180";

    /// <summary>Código do responsável pela retenção do ICMS ST. Valores: 1-Remetente Direto, 2-Remetente Indireto, 3-Próprio declarante.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public CodigoResponsavelRetencaoSt? CodRespRet { get; set; }

    /// <summary>Quantidade do item convertida na unidade de controle de estoque informada no Registro 0200.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? QuantConv { get; set; }

    /// <summary>Unidade adotada para informar o campo QUANT_CONV, conforme Registro 0190.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor unitário da mercadoria considerando a unidade utilizada para informar QUANT_CONV.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlUnitConv { get; set; }

    /// <summary>Valor unitário do ICMS operação própria que o informante teria direito ao crédito caso a mercadoria estivesse sob regime comum de tributação.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlUnitIcmsOpConv { get; set; }

    /// <summary>Valor unitário da base de cálculo do imposto pago ou retido anteriormente por substituição, aplicando-se redução, se houver.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlUnitBcIcmsStConv { get; set; }

    /// <summary>Valor unitário do imposto pago ou retido anteriormente por substituição, inclusive FCP se devido.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlUnitIcmsStConv { get; set; }

    /// <summary>Valor unitário do FCP ST agregado ao valor informado no campo VL_UNIT_ICMS_ST_CONV.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 6)]
    public decimal? VlUnitFcpStConv { get; set; }

    /// <summary>Código do modelo do documento de arrecadação. Valores: 0-Doc. estadual; 1-GNRE.</summary>
    [CampoSped(Ordem = 10, Tamanho = 1)]
    public CodigoModeloDocumentoArrecadacao? CodDa { get; set; }

    /// <summary>Número do documento de arrecadação estadual, se houver.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0)]
    public string? NumDa { get; set; }
}
