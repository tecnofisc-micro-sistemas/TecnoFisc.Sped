namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de receita — campo IND_REC nos registros de NF/Conta de Energia Elétrica,
/// Água e Gás do EFD ICMS-IPI (ex.: C510).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 137.
/// </summary>
public enum IndicadorTipoReceita
{
    /// <summary>0 — Receita própria.</summary>
    ReceitaPropria = 0,

    /// <summary>1 — Receita de terceiros.</summary>
    ReceitaTerceiros = 1,
}
