using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I155 — Detalhe dos Saldos Periódicos. Nível hierárquico 4, ocorrência 0:N por I150.
/// Informa os saldos de cada conta analítica/centro de custos no período determinado pelo I150:
/// saldo inicial, total de débitos, total de créditos e saldo final.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 134–140.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item>Obrigatório quando existe registro I150.</item>
///   <item><c>REGRA_IND_DC_INI_OBRIGATORIO</c>: <see cref="IndDcIni"/> deve ser preenchido mesmo
///   quando <see cref="VlSldIni"/> for zero.</item>
///   <item><c>REGRA_IND_DC_FIN_OBRIGATORIO</c>: <see cref="IndDcFin"/> deve ser preenchido mesmo
///   quando <see cref="VlSldFin"/> for zero.</item>
///   <item><c>REGRA_VALIDACAO_SALDO_FINAL</c>: VL_SLD_FIN = VL_SLD_INI ± VL_DEB ∓ VL_CRED
///   (considerando os indicadores de débito/crédito).</item>
///   <item><c>REGRA_DUPLICIDADE_CONTA_SALDO_PERIODICO</c>: a chave
///   <see cref="CodCta"/> + <see cref="CodCcus"/> não pode ser duplicada para o mesmo I150.</item>
///   <item>Campos adicionais de moeda funcional (<c>VL_SLD_INI_MF</c> a <c>IND_DC_FIN_MF</c>)
///   são criados via Registro I020 e presentes somente quando <c>IDENT_MF = "S"</c> no Registro 0000.
///   Não fazem parte do leiaute fixo — o parser os descarta silenciosamente.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I155", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI155 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I155";

    /// <summary>
    /// Código da conta analítica.
    /// Campo <c>COD_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código do centro de custos.
    /// Campo <c>COD_CCUS</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Valor do saldo inicial do período. Quando zero, preencher com "0" ou "0,00".
    /// Campo <c>VL_SLD_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldIni { get; set; }

    /// <summary>
    /// Indicador da situação do saldo inicial (D = Devedor; C = Credor).
    /// Campo <c>IND_DC_INI</c>.
    /// <c>REGRA_IND_DC_INI_OBRIGATORIO</c>: obrigatório mesmo quando o saldo for zero.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorDebitoCredito? IndDcIni { get; set; }

    /// <summary>
    /// Valor total dos débitos do período. Quando zero, preencher com "0" ou "0,00".
    /// Campo <c>VL_DEB</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlDeb { get; set; }

    /// <summary>
    /// Valor total dos créditos do período. Quando zero, preencher com "0" ou "0,00".
    /// Campo <c>VL_CRED</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCred { get; set; }

    /// <summary>
    /// Valor do saldo final do período. Quando zero, preencher com "0" ou "0,00".
    /// Campo <c>VL_SLD_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldFin { get; set; }

    /// <summary>
    /// Indicador da situação do saldo final (D = Devedor; C = Credor).
    /// Campo <c>IND_DC_FIN</c>.
    /// <c>REGRA_IND_DC_FIN_OBRIGATORIO</c>: obrigatório mesmo quando o saldo for zero.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 1)]
    public IndicadorDebitoCredito? IndDcFin { get; set; }
}
