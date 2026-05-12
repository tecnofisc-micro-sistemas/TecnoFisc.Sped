namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de operação — campo OPER no Registro C105 (ICMS ST recolhido para UF
/// diversa do destinatário, código 55).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 66.
/// </summary>
public enum IndicadorOperacaoIcmsSt
{
    /// <summary>0 — Combustíveis e Lubrificantes.</summary>
    CombustiveisLubrificantes = 0,

    /// <summary>1 — Leasing de veículos ou faturamento direto.</summary>
    LeasingOuFaturamentoDireto = 1,
}
