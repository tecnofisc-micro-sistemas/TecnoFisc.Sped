namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo da arma de fogo — campo IND_ARM no Registro C174
/// (Operações com Armas de Fogo, cód. 01).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 84.
/// </summary>
public enum IndicadorTipoArmaFogo
{
    /// <summary>0 — Uso permitido.</summary>
    UsoPermitido = 0,

    /// <summary>1 — Uso restrito.</summary>
    UsoRestrito = 1,
}
