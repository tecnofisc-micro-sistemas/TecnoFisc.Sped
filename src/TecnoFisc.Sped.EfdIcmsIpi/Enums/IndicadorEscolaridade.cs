namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador de escolaridade do profissional — campo <c>IND_ESC</c> do Registro B510.
/// Exclusivo do Bloco B (ISS Uniprofissional); não regido pelo Ato COTEPE/ICMS.
/// </summary>
public enum IndicadorEscolaridade
{
    /// <summary>0 — Nível superior.</summary>
    NivelSuperior = 0,

    /// <summary>1 — Nível médio.</summary>
    NivelMedio = 1,
}
