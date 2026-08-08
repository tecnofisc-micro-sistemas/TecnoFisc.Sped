using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I355 — Detalhes dos Saldos das Contas de Resultado Antes do Encerramento.
/// Nível hierárquico 4, ocorrência 0:N por I350.
/// Informa o valor do saldo final de cada conta de resultado antes dos lançamentos de
/// encerramento. Filho do Registro I350 (que identifica a data de apuração).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 158–159.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_COD_CTA_DT_RES_DUPLICIDADE</c>: a chave <see cref="CodCta"/>+<see cref="CodCcus"/>
///   não pode ser duplicada para o mesmo I350 (mesma data de apuração).</item>
///   <item><c>REGRA_VALIDACAO_CONTA_RESULTADO</c>: <see cref="CodCta"/> deve referenciar uma conta
///   analítica de resultado existente no Plano de Contas (I050).</item>
///   <item><c>REGRA_VALIDACAO_SALDO_CONTA</c>: a soma dos lançamentos de encerramento do tipo "E"
///   (I200.IND_LCTO) para a conta e data deve ser igual ao valor de <see cref="VlCta"/> (com
///   indicador de débito/crédito invertido), para escriturações do tipo G ou R.</item>
///   <item><c>REGRA_CCUS_NO_CENTRO_CUSTOS</c>: quando preenchido, <see cref="CodCcus"/> deve
///   existir no Registro I100 (Centro de Custos).</item>
///   <item>Campos adicionais de moeda funcional (<c>VL_CTA_MF</c>, <c>IND_DC_MF</c>) são criados
///   via Registro I020 e presentes somente quando <c>IDENT_MF = "S"</c> no Registro 0000.
///   Não fazem parte do leiaute fixo — o parser os descarta silenciosamente.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I355", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI355 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I355";

    /// <summary>
    /// Código da conta analítica de resultado. Chave do registro (junto com <see cref="CodCcus"/>).
    /// Campo <c>COD_CTA</c>. Referencia I050 (<c>REGRA_VALIDACAO_CONTA_RESULTADO</c>).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código do centro de custos.
    /// Campo <c>COD_CCUS</c>. Referencia I100 (<c>REGRA_CCUS_NO_CENTRO_CUSTOS</c>).
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Valor do saldo final antes do lançamento de encerramento.
    /// Campo <c>VL_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCta { get; set; }

    /// <summary>
    /// Indicador da situação do saldo final (D = Devedor; C = Credor).
    /// Campo <c>IND_DC</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDc { get; set; }
}
