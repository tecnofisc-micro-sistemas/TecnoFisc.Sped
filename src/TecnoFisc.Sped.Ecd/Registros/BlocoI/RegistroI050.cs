using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I050 — Plano de Contas. Nível hierárquico 3, ocorrência 1:N por arquivo.
/// Discrimina todas as contas sintéticas e analíticas do plano de contas da pessoa jurídica,
/// devendo conter no mínimo 4 níveis (conforme CTG 2001 R3 e art. 177 a 182 da Lei n.º 6.404/1976).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 117–122.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_COD_CTA_DUPLICADO</c>: <see cref="CodCta"/> não pode ser duplicado no plano de contas.</item>
///   <item><c>REGRA_AGL_CCUS_VAZIO_PREENCHIDO</c>: filhos I052 do mesmo I050 não podem misturar COD_CCUS vazio e preenchido.</item>
///   <item><c>REGRA_I051_OBRIGATORIO</c>: quando COD_PLAN_REF do Registro 0000 for informado, o mapeamento em I051 é obrigatório.</item>
///   <item><c>REGRA_ANO_ALT_MAIOR_ANO_FIN</c>: o ano de <see cref="DtAlt"/> não deve ser maior que o ano de DT_FIN do Registro 0000.</item>
///   <item><c>REGRA_TABELA_NATUREZA</c>: <see cref="CodNat"/> deve existir na Tabela de Naturezas das Contas/Grupo de Contas.</item>
///   <item><c>REGRA_MAIOR_QUE_UM</c>: <see cref="Nivel"/> deve ser maior ou igual a 1.</item>
///   <item><c>REGRA_VALIDA_NIVEL_CONTAS</c>: quando IND_ESC (I010) = G, R ou B e <see cref="IndCta"/> = A,
///   <see cref="CodNat"/> ∈ {01, 02, 03} implica <see cref="Nivel"/> ≥ 4.</item>
///   <item><c>REGRA_COD_CTA_IGUAL_COD_CTA_SUP</c>: <see cref="CodCta"/> ≠ <see cref="CodCtaSup"/>.</item>
///   <item><c>REGRA_COD_CTA_SUP_OBRIGATORIO</c>: quando <see cref="Nivel"/> &gt; 1, <see cref="CodCtaSup"/> é obrigatório.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I050", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI050 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I050";

    /// <summary>
    /// Data da inclusão/alteração da conta no plano de contas (ddMMyyyy).
    /// Campo <c>DT_ALT</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    /// <summary>
    /// Código da natureza da conta/grupo de contas, conforme tabela publicada pelo Sped:
    /// 01 = Contas de Ativo, 02 = Contas de Passivo, 03 = Patrimônio Líquido,
    /// 04 = Contas de Resultado, 05 = Contas de Compensação, 09 = Outras.
    /// Campo <c>COD_NAT</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodNat { get; set; }

    /// <summary>
    /// Indicador do tipo de conta: S = Sintética (grupo de contas), A = Analítica (conta).
    /// Campo <c>IND_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoConta IndCta { get; set; }

    /// <summary>
    /// Nível da conta analítica/grupo de contas. Número crescente a partir da conta/grupo de
    /// menor detalhamento (Ativo, Passivo, etc.); deve ser acrescido de 1 a cada mudança de nível.
    /// Campo <c>NIVEL</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int Nivel { get; set; }

    /// <summary>
    /// Código da conta analítica/grupo de contas.
    /// Campo <c>COD_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código da conta sintética/grupo de contas de nível imediatamente superior.
    /// Obrigatório quando <see cref="Nivel"/> &gt; 1.
    /// Campo <c>COD_CTA_SUP</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? CodCtaSup { get; set; }

    /// <summary>
    /// Nome da conta analítica/grupo de contas.
    /// Campo <c>CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Obrigatorio = true)]
    public string? Cta { get; set; }
}
