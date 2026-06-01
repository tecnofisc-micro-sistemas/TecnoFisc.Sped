using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador da situação do saldo (débito/crédito) — campos <c>IND_DC_INI_REC</c> e
/// <c>IND_DC_FIN_REC</c> do Registro C155 da ECD.
/// D = Devedor; C = Credor.
/// </summary>
public enum IndicadorDebitoCredito
{
    /// <summary>D — Devedor.</summary>
    [SpedValor("D")]
    Devedor = 0,

    /// <summary>C — Credor.</summary>
    [SpedValor("C")]
    Credor = 1,
}
