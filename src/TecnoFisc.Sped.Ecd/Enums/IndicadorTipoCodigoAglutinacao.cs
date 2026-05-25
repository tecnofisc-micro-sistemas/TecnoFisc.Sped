using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador do tipo de código de aglutinação — campo <c>IND_COD_AGL</c> dos Registros
/// J100, J150 e J210 da ECD.
/// T = Totalizador (nível que totaliza níveis inferiores); D = Detalhe (nível mais granular).
/// </summary>
public enum IndicadorTipoCodigoAglutinacao
{
    /// <summary>T — Totalizador (nível que totaliza um ou mais níveis inferiores da demonstração financeira).</summary>
    [SpedValor("T")]
    Totalizador = 0,

    /// <summary>D — Detalhe (nível mais detalhado da demonstração financeira, ligado a contas analíticas).</summary>
    [SpedValor("D")]
    Detalhe = 1,
}
