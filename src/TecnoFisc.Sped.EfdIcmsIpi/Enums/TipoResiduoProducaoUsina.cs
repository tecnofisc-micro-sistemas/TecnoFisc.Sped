using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Tipo de residuo produzido no Registro 1391.
/// Valores conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 280-281.
/// </summary>
public enum TipoResiduoProducaoUsina
{
    /// <summary>01 - Bagaco de cana.</summary>
    [SpedValor("01")]
    BagacoCana = 1,

    /// <summary>02 - DDG.</summary>
    [SpedValor("02")]
    Ddg = 2,

    /// <summary>03 - WDG.</summary>
    [SpedValor("03")]
    Wdg = 3,
}
