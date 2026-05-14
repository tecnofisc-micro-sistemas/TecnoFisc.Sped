namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Natureza da exportacao — campo NAT_EXP do Registro 1100.
/// Valores conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 269.
/// </summary>
public enum NaturezaExportacao
{
    /// <summary>0 - Exportacao direta.</summary>
    Direta = 0,

    /// <summary>1 - Exportacao indireta.</summary>
    Indireta = 1,
}
