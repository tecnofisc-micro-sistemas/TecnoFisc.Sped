using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I053 — Subcontas Correlatas. Nível hierárquico 4, ocorrência 0:N.
/// Demonstra os grupos (identificados por <c>COD_IDT</c>) compostos de uma conta "pai" e uma ou
/// mais subcontas correlatas. O mesmo <c>COD_IDT</c> pode ser reutilizado para mais de um conjunto
/// de conta "pai" e subconta(s). Somente para contas analíticas (I050.IND_CTA = "A").
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 127–129.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_PARA_CONTA_ANALITICA</c>: I053 somente pode existir quando
///   I050.IND_CTA = "A" (conta analítica).</item>
///   <item><c>REGRA_COD_IDT_COD_CTA_DUPLICIDADE</c>: a chave COD_IDT + COD_CTA não pode ser
///   duplicada para o mesmo I050.</item>
///   <item><c>REGRA_SUB_CONTA_PAI</c>: a subconta informada em <see cref="CodCntCorr"/>, quando
///   aparece no Registro I050 (COD_CTA), não pode possuir filhos I053.</item>
///   <item><c>REGRA_COD_IDT_UNICO_POR_CONTA</c>: todos os filhos I053 de um mesmo I050 devem
///   compartilhar o mesmo <see cref="CodIdt"/>.</item>
///   <item><c>REGRA_SUBCONTA_NO_PLANO_CONTAS</c>: <see cref="CodCntCorr"/> deve existir no
///   plano de contas (I050.COD_CTA).</item>
///   <item><c>REGRA_NAT_090_UNICA_POR_CONTA</c>: no máximo duas subcontas de natureza 90, 91,
///   92 ou 93 (<see cref="NatSubCnt"/>) por conta (I050.COD_CTA).</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I053", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI053 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I053";

    /// <summary>
    /// Código de identificação do grupo de conta-subconta(s). Permite agrupar uma conta "pai"
    /// com uma ou mais subcontas correlatas; o mesmo código pode ser usado em mais de um conjunto.
    /// Campo <c>COD_IDT</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodIdt { get; set; }

    /// <summary>
    /// Código da subconta correlata. Deve estar no plano de contas e só pode estar relacionada
    /// a um único grupo (<see cref="CodIdt"/>).
    /// Campo <c>COD_CNT_CORR</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCntCorr { get; set; }

    /// <summary>
    /// Natureza da subconta correlata, conforme Tabela de Natureza da Subconta publicada no Sped.
    /// Campo <c>NAT_SUB_CNT</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public NaturezaSubcontaCorrelata? NatSubCnt { get; set; }
}
