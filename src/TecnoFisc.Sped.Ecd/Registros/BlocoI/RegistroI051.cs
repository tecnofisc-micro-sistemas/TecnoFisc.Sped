using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Registros.Bloco0;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I051 — Plano de Contas Referencial. Nível hierárquico 4, ocorrência 0:N.
/// Estabelece o mapeamento (DE-PARA) entre as contas analíticas do plano de contas da pessoa
/// jurídica (I050) e o plano de contas referencial informado no campo <c>COD_PLAN_REF</c>
/// do <see cref="Registro0000"/>. Preenchimento obrigatório quando <c>COD_PLAN_REF</c> for
/// informado no Registro 0000. Somente para contas analíticas (I050.IND_CTA = "A").
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 123–124.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_PARA_CONTA_ANALITICA</c>: I051 somente pode existir quando o
///   campo <see cref="RegistroI050.IndCta"/> = A (conta analítica).</item>
///   <item><c>REGRA_NATUREZA_CONTA_DIFERENTE</c>: verifica coerência entre a natureza da conta pai
///   (I050.COD_NAT) e o <see cref="CodCtaRef"/> informado.</item>
///   <item><c>REGRA_CCUS_NO_CENTRO_CUSTOS_N3</c>: <see cref="CodCcus"/> deve existir no Registro I100
///   (Centro de Custos), quando informado.</item>
///   <item><c>REGRA_VALIDADE_COD_CTA_PAD</c>: período de validade do <see cref="CodCtaRef"/> deve
///   estar dentro do período da escrituração (gera aviso).</item>
///   <item><c>REGRA_NAO_EXISTE_COD_CTA_PAD</c>: <see cref="CodCtaRef"/> deve existir no plano
///   de contas referencial (gera aviso).</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I051", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI051 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I051";

    /// <summary>
    /// Código do centro de custo. Preencher somente quando interferir na identificação do código
    /// do plano de contas referencial. Quando a vinculação da conta com o código referencial
    /// independer do centro de custos, este deve ser informado apenas no Registro I100.
    /// Campo <c>COD_CCUS</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Código da conta de acordo com o plano de contas referencial, conforme tabela publicada
    /// pelos órgãos indicados no campo <c>COD_PLAN_REF</c> do <see cref="Registro0000"/>.
    /// Campo <c>COD_CTA_REF</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaRef { get; set; }
}
