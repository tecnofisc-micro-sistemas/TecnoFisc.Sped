using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K210 — Mapeamento para Planos de Contas das Empresas Consolidadas. Nível hierárquico
/// 3, ocorrência 1:N por K200. Apresenta o mapeamento das contas analíticas do plano de contas
/// consolidado (K200) para as contas dos planos de contas das empresas consolidadas.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 222–223.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_CONTA_CONSOLIDADA_ANALITICA</c>: a conta consolidada no K200 pai deve ter
///   indicador de tipo de conta (<c>IND_CTA</c>) igual a "A" — Analítica. O PGE gera erro caso
///   contrário.</item>
///   <item><c>REGRA_EXISTE_EMP_COD_K100</c>: <see cref="CodEmp"/> deve ter sido informado
///   no campo <c>EMP_COD</c> (Campo 03) de algum registro K100 do arquivo.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K210", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K210";

    /// <summary>
    /// Código de identificação da empresa participante. Deve corresponder a um <c>EMP_COD</c>
    /// informado em K100.
    /// Campo <c>COD_EMP</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public int CodEmp { get; set; }

    /// <summary>
    /// Código da conta da empresa participante no plano de contas próprio dessa empresa.
    /// Campo <c>COD_CTA_EMP</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaEmp { get; set; }
}
