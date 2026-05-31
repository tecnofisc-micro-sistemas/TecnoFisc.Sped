using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I052 — Indicação dos Códigos de Aglutinação. Nível hierárquico 4, ocorrência 0:N.
/// Informa o código de aglutinação (<c>COD_AGL</c>) válido na data de encerramento, utilizado
/// nas demonstrações contábeis do Bloco J. Deve ser informado somente para contas analíticas do
/// plano de contas (I050.IND_CTA = "A").
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 125–126.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_PARA_CONTA_ANALITICA</c>: I052 somente pode existir quando
///   I050.IND_CTA = "A" (conta analítica).</item>
///   <item><c>REGRA_COD_CCUS_COD_AGL_DUPLICIDADE</c>: a chave COD_CCUS + COD_AGL não pode
///   ser duplicada no mesmo I050.</item>
///   <item><c>REGRA_EXISTE_I052_CTA_SINTETICA</c>: I052 não pode ser filho de I050 com
///   IND_CTA = "S" (sintética).</item>
///   <item><c>REGRA_AGLUTINACAO_EM_SINTETICA</c>: COD_AGL não pode ser o mesmo que um código
///   de aglutinação cadastrado nas linhas sintéticas dos registros J100 ou J150 (totalizador)
///   cujo J005 abranja o período da escrituração.</item>
///   <item><c>REGRA_CCUS_NO_CENTRO_CUSTOS_N3</c>: <see cref="CodCcus"/>, quando informado,
///   deve existir no Registro I100 (Centro de Custos).</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I052", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI052 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I052";

    /// <summary>
    /// Código do centro de custo. Preencher somente quando interferir na identificação do código
    /// de aglutinação; caso contrário, informar apenas no Registro I100.
    /// Campo <c>COD_CCUS</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Código de aglutinação utilizado nas demonstrações contábeis do Bloco J.
    /// Somente para contas analíticas.
    /// Campo <c>COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodAgl { get; set; }
}
