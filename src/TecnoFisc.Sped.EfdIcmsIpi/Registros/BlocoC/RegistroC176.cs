using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C176 — Complemento de Item: Ressarcimento de ICMS e FCP em Operações com
/// Substituição Tributária (cód. 01, 55).
/// Nível hierárquico 4, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 85.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 itens 16-18 + 3.0.9 itens 1-2):</b> campos 12-15 passaram de OC para O;
/// nova orientação de preenchimento para os campos 12, 14 e 15; descrição do campo 18
/// <c>COD_RESP_RET</c> reescrita; validação do campo 14 <c>VL_UNIT_LIMITE_BC_ICMS_ULT_E</c>
/// retira a condição "&gt;0" e passa a exigir <c>COD_RESP_RET = "2"</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "C176", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC176 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C176";

    /// <summary>Código do modelo do documento fiscal relativo à última entrada. Valores válidos: 01, 55.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodModUltE { get; set; }

    /// <summary>Número do documento fiscal relativo à última entrada.</summary>
    [CampoSped(Ordem = 3, Tamanho = 9, Obrigatorio = true)]
    public int? NumDocUltE { get; set; }

    /// <summary>Série do documento fiscal relativo à última entrada.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3)]
    public string? SerUltE { get; set; }

    /// <summary>Data relativa à última entrada da mercadoria (formato DDMMAAAA).</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtUltE { get; set; }

    /// <summary>Código do participante (emitente do documento relativo à última entrada), conforme Registro 0150.</summary>
    [CampoSped(Ordem = 6, Tamanho = 60, Obrigatorio = true)]
    public string? CodPartUltE { get; set; }

    /// <summary>Quantidade do item relativo à última entrada.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal? QuantUltE { get; set; }

    /// <summary>Valor unitário da mercadoria constante na NF relativa à última entrada, inclusive despesas acessórias.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal? VlUnitUltE { get; set; }

    /// <summary>Valor unitário da base de cálculo do imposto pago por substituição.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal? VlUnitBcSt { get; set; }

    /// <summary>Número completo da chave da NF-e relativo à última entrada. Obrigatório quando COD_MOD = 55.</summary>
    [CampoSped(Ordem = 10, Tamanho = 44)]
    public ChaveAcesso? ChaveNfeUltE { get; set; }

    /// <summary>Número sequencial do item na NF entrada que corresponde à mercadoria objeto de pedido de ressarcimento.</summary>
    [CampoSped(Ordem = 11, Tamanho = 3)]
    public int? NumItemUltE { get; set; }

    /// <summary>Valor unitário da base de cálculo da operação própria do remetente sob o regime comum de tributação.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlUnitBcIcmsUltE { get; set; }

    /// <summary>Alíquota do ICMS aplicável à última entrada da mercadoria.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? AliqIcmsUltE { get; set; }

    /// <summary>Valor unitário da base de cálculo do ICMS relativo à última entrada, limitado ao valor da BC da retenção.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlUnitLimiteBcIcmsUltE { get; set; }

    /// <summary>Valor unitário do crédito de ICMS sobre operações próprias do remetente relativo à última entrada da mercadoria.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitIcmsUltE { get; set; }

    /// <summary>Alíquota do ICMS ST relativa à última entrada da mercadoria.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? AliqStUltE { get; set; }

    /// <summary>Valor unitário do ressarcimento (parcial ou completo) de ICMS decorrente da quebra da ST.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitRes { get; set; }

    /// <summary>Código do responsável pela retenção do ICMS ST.</summary>
    [CampoSped(Ordem = 18, Tamanho = 1)]
    public CodigoResponsavelRetencaoSt? CodRespRet { get; set; }

    /// <summary>Código do motivo do ressarcimento.</summary>
    [CampoSped(Ordem = 19, Tamanho = 1)]
    public CodigoMotivoRessarcimentoSt? CodMotRes { get; set; }

    /// <summary>Chave da NF-e emitida pelo substituto na qual consta o valor do ICMS ST retido.</summary>
    [CampoSped(Ordem = 20, Tamanho = 44)]
    public ChaveAcesso? ChaveNfeRet { get; set; }

    /// <summary>Código do participante emitente da NF-e em que houve a retenção do ICMS ST (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 21, Tamanho = 60)]
    public string? CodPartNfeRet { get; set; }

    /// <summary>Série da NF-e em que houve a retenção do ICMS ST.</summary>
    [CampoSped(Ordem = 22, Tamanho = 3)]
    public string? SerNfeRet { get; set; }

    /// <summary>Número da NF-e em que houve a retenção do ICMS ST.</summary>
    [CampoSped(Ordem = 23, Tamanho = 9)]
    public int? NumNfeRet { get; set; }

    /// <summary>Número sequencial do item na NF-e em que houve a retenção do ICMS ST.</summary>
    [CampoSped(Ordem = 24, Tamanho = 3)]
    public int? ItemNfeRet { get; set; }

    /// <summary>Código do modelo do documento de arrecadação.</summary>
    [CampoSped(Ordem = 25, Tamanho = 1)]
    public CodigoModeloDocumentoArrecadacao? CodDa { get; set; }

    /// <summary>Número do documento de arrecadação estadual, se houver.</summary>
    [CampoSped(Ordem = 26, Tamanho = 0)]
    public string? NumDa { get; set; }

    /// <summary>Valor unitário do ressarcimento (parcial ou completo) de FCP decorrente da quebra da ST.</summary>
    [CampoSped(Ordem = 27, Tamanho = 0, Decimais = 3)]
    public decimal? VlUnitResFcpSt { get; set; }
}
