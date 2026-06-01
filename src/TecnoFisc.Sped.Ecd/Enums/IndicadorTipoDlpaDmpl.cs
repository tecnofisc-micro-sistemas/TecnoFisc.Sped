using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador do tipo de demonstração — campo <c>IND_TIP</c> do Registro J210 da ECD.
/// 0 = DLPA – Demonstração de Lucros ou Prejuízos Acumulados;
/// 1 = DMPL – Demonstração de Mutações do Patrimônio Líquido.
/// </summary>
public enum IndicadorTipoDlpaDmpl
{
    /// <summary>0 — DLPA – Demonstração de Lucros ou Prejuízos Acumulados.</summary>
    [SpedValor("0")]
    Dlpa = 0,

    /// <summary>1 — DMPL – Demonstração de Mutações do Patrimônio Líquido.</summary>
    [SpedValor("1")]
    Dmpl = 1,
}
