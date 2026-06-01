using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K310 — Empresas Detentoras das Parcelas do Valor Eliminado Total. Nível hierárquico 4,
/// ocorrência 0:N por K300. Identifica cada empresa detentora do valor aglutinado que foi eliminado
/// e a respectiva parcela eliminada.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 225–226.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_EXISTE_EMP_COD_K100</c>: <see cref="EmpCodParte"/> deve existir em K100,
///   no campo código de identificação da empresa participante (<c>EMP_COD</c>). O PGE gera erro
///   caso contrário.</item>
///   <item><c>REGRA_SOMATORIO_VALOR_CONTRAPARTIDA</c>: o somatório de <see cref="Valor"/> do
///   K310 com a contrapartida do valor total eliminado — VALOR (Campo 04) — do registro K315
///   deve ser igual a zero, considerando os indicadores de situação do valor eliminado
///   (D – Devedor ou C – Credor). O PGE gera erro caso contrário.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K310", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K310";

    /// <summary>
    /// Código da empresa detentora do valor aglutinado que foi eliminado.
    /// Campo <c>EMP_COD_PARTE</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public int EmpCodParte { get; set; }

    /// <summary>
    /// Parcela do valor eliminado total.
    /// Campo <c>VALOR</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal Valor { get; set; }

    /// <summary>
    /// Indicador da situação do valor eliminado: D – Devedor; C – Credor.
    /// Campo <c>IND_VALOR</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValor { get; set; }
}
