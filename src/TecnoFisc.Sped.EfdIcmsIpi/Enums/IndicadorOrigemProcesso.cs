namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador da origem do processo judicial ou administrativo — campo IND_PROC do Registro C111.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 67.
/// </summary>
public enum IndicadorOrigemProcesso
{
    /// <summary>0 — SEFAZ.</summary>
    Sefaz = 0,

    /// <summary>1 — Justiça Federal.</summary>
    JusticaFederal = 1,

    /// <summary>2 — Justiça Estadual.</summary>
    JusticaEstadual = 2,

    /// <summary>3 — SECEX/SRF.</summary>
    SecexSrf = 3,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
