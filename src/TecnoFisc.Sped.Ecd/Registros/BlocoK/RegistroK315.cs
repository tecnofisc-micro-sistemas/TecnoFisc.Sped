using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K315 — Empresas Contrapartes das Parcelas do Valor Eliminado Total. Nível hierárquico 5,
/// ocorrência 0:N por K310. Identifica cada empresa contraparte do valor eliminado e a respectiva
/// parcela da contrapartida.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 226–228.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_EXISTE_EMP_COD_K100</c>: <see cref="EmpCodContra"/> deve existir em K100,
///   no campo código de identificação da empresa participante (<c>EMP_COD</c>). O PGE gera erro
///   caso contrário.</item>
///   <item><c>REGRA_EMP_COD_CONTRA_DIFERENTE_EMP_COD_PARTE</c>: <see cref="EmpCodContra"/> deve
///   ser diferente do campo <c>EMP_COD_PARTE</c> (Campo 02) do registro K310 pai. O PGE gera erro
///   caso contrário.</item>
///   <item><c>REGRA_COD_CTA_DIFERENTE_COD_CONTRA</c>: <see cref="CodContra"/> deve ser diferente
///   do código da conta consolidada (<c>COD_CTA</c>, Campo 02) do registro K300 avô. O PGE gera
///   aviso caso contrário.</item>
///   <item><c>REGRA_EXISTE_COD_CTA_K300</c>: <see cref="CodContra"/> deve existir em algum
///   registro K300, no campo código da conta consolidada (<c>COD_CAT</c>, Campo 02). O PGE gera
///   erro caso contrário.</item>
///   <item><c>REGRA_MAIOR_QUE_ZERO</c>: <see cref="Valor"/> deve ser maior que zero. O PGE gera
///   erro caso contrário.</item>
///   <item><c>REGRA_SOMATORIO_VALOR_CONTRAPARTIDA</c>: o somatório de <see cref="Valor"/> do
///   K315 com a parcela do valor total eliminado — VALOR (Campo 03) — do registro K310 pai deve
///   ser igual a zero, considerando os indicadores de situação do valor eliminado
///   (D – Devedor ou C – Credor). O PGE gera erro caso contrário.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K315", Nivel = 5, Bloco = "K")]
public sealed partial class RegistroK315 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K315";

    /// <summary>
    /// Código da empresa da contrapartida.
    /// Campo <c>EMP_COD_CONTRA</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public int EmpCodContra { get; set; }

    /// <summary>
    /// Código da conta consolidada da contrapartida.
    /// Campo <c>COD_CONTRA</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodContra { get; set; }

    /// <summary>
    /// Parcela da contrapartida do valor eliminado total.
    /// Campo <c>VALOR</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal Valor { get; set; }

    /// <summary>
    /// Indicador da situação do valor eliminado: D – Devedor; C – Credor.
    /// Campo <c>IND_VALOR</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValor { get; set; }
}
