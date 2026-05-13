namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo do frete da operação de redespacho — campo IND_FRT_RED do Registro D130
/// (CT Rodoviário de Cargas, cód. 08 e 8B).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 170.
/// </summary>
public enum IndicadorFreteRedespacho
{
    /// <summary>0 — Sem redespacho.</summary>
    SemRedespacho = 0,

    /// <summary>1 — Por conta do emitente.</summary>
    PorContaDoEmitente = 1,

    /// <summary>2 — Por conta do destinatário.</summary>
    PorContaDoDestinatario = 2,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
