using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I157 — Transferência de Saldos de Plano de Contas Anterior. Nível hierárquico 5, ocorrência 0:N por I155.
/// Informa as transferências de saldos das contas analíticas do plano de contas anterior para as contas
/// analíticas do plano de contas novo, quando não forem realizados lançamentos contábeis transferindo
/// o saldo da conta analítica antiga para a conta analítica nova nos registros I200 e I250.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 140–142.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item>Registro facultativo.</item>
///   <item><c>REGRA_IND_DC_INI_OBRIGATORIO</c>: <see cref="IndDcIni"/> deve ser preenchido mesmo
///   quando <see cref="VlSldIni"/> for zero.</item>
///   <item><c>REGRA_VALIDA_SLD_INI_SOMA_SLD_INI_I157_MF</c>: o valor de <c>VL_SLD_INI_MF</c>
///   do I150 deve ser igual à soma dos <see cref="VlSldIni"/> de todos os registros I157 filhos do I155
///   correspondente (quando <c>IDENT_MF = "S"</c> no Registro 0000).</item>
///   <item><c>REGRA_CONTA_I157_INEXISTENTE</c>: se houve recuperação de ECD anterior e mudança de
///   plano de contas, o <see cref="CodCta"/> + <see cref="CodCcus"/> devem existir no registro C155.</item>
///   <item><c>REGRA_EXISTE_I157_PERIODO_ANTERIOR</c>: caso o I157 tenha sido preenchido, a data
///   inicial do período no I150 deve ser igual à data inicial da ECD informada no Registro 0000.</item>
///   <item>Campos adicionais de moeda funcional (<c>VL_SLD_INI_MF</c>, <c>IND_DC_INI_MF</c>)
///   são criados via Registro I020 e presentes somente quando <c>IDENT_MF = "S"</c> no Registro 0000.
///   Não fazem parte do leiaute fixo — o parser os descarta silenciosamente.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I157", Nivel = 5, Bloco = "I")]
public sealed partial class RegistroI157 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I157";

    /// <summary>
    /// Código da conta analítica do plano de contas anterior.
    /// Campo <c>COD_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código do centro de custos do plano de contas anterior.
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
}
