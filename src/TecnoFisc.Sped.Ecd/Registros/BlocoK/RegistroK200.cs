using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K200 — Plano de Contas Consolidado. Nível hierárquico 2, ocorrência 1:N por arquivo.
/// Apresenta o plano de contas utilizado nas escriturações contábeis consolidadas.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 220–222.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_TABELA_NATUREZA</c>: <see cref="CodNat"/> deve existir na Tabela de Naturezas
///   das Contas/Grupo de Contas (01, 02, 03, 04, 05, 09).</item>
///   <item><c>REGRA_MAIOR_QUE_UM</c>: <see cref="Nivel"/> deve ser maior ou igual a 1.</item>
///   <item><c>REGRA_ANALITICA_NIVEL_2</c>: quando <see cref="IndCta"/> = A (Analítica) e
///   <see cref="CodNat"/> ∈ {01, 02, 03, 04}, <see cref="Nivel"/> deve ser maior que 2.</item>
///   <item><c>REGRA_COD_CTA_SUP_OBRIGATORIO</c>: quando <see cref="Nivel"/> &gt; 1,
///   <see cref="CodCtaSup"/> é obrigatório.</item>
///   <item><c>REGRA_CONTA_SUPERIOR_NAO_SE_APLICA</c>: quando <see cref="Nivel"/> = 1,
///   <see cref="CodCtaSup"/> não deve ser informado.</item>
///   <item><c>REGRA_CTA_CONSOLIDADA_DE_NIVEL_SUPERIOR_INVALIDA</c>: quando <see cref="Nivel"/> &gt; 1,
///   <see cref="CodCtaSup"/> deve existir no plano de contas (K200), a conta superior deve ser
///   Sintética e ter nível inferior ao atual.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K200", Nivel = 2, Bloco = "K")]
public sealed partial class RegistroK200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K200";

    /// <summary>
    /// Código da natureza da conta/grupo de contas, conforme tabela publicada pelo Sped:
    /// 01 = Contas de Ativo, 02 = Contas de Passivo, 03 = Patrimônio Líquido,
    /// 04 = Contas de Resultado, 05 = Contas de Compensação, 09 = Outras.
    /// Campo <c>COD_NAT</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodNat { get; set; }

    /// <summary>
    /// Indicador do tipo de conta: S = Sintética (grupo de contas), A = Analítica (conta).
    /// Campo <c>IND_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoConta IndCta { get; set; }

    /// <summary>
    /// Nível da conta no plano de contas consolidado. Deve ser maior ou igual a 1.
    /// Campo <c>NIVEL</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public int Nivel { get; set; }

    /// <summary>
    /// Código da conta consolidada.
    /// Campo <c>COD_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código da conta consolidada de nível imediatamente superior.
    /// Obrigatório quando <see cref="Nivel"/> &gt; 1; não deve ser informado quando <see cref="Nivel"/> = 1.
    /// Campo <c>COD_CTA_SUP</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? CodCtaSup { get; set; }

    /// <summary>
    /// Nome da conta consolidada.
    /// Campo <c>CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? Cta { get; set; }
}
