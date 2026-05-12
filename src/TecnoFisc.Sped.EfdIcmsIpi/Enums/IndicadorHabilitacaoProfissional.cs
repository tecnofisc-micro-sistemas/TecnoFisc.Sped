namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador de habilitação do profissional — campo <c>IND_PROF</c> do Registro B510.
/// Exclusivo do Bloco B (ISS Uniprofissional); não regido pelo Ato COTEPE/ICMS.
/// </summary>
public enum IndicadorHabilitacaoProfissional
{
    /// <summary>0 — Profissional habilitado.</summary>
    Habilitado = 0,

    /// <summary>1 — Profissional não habilitado.</summary>
    NaoHabilitado = 1,
}
