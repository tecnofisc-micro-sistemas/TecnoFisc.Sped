using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Tipo de medicao da bomba no Registro 1350.
/// Valores conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 278-279.
/// </summary>
public enum TipoMedicaoBomba
{
    /// <summary>0 - Analogico.</summary>
    [SpedValor("0")]
    Analogico = 0,

    /// <summary>1 - Digital.</summary>
    [SpedValor("1")]
    Digital = 1,
}
