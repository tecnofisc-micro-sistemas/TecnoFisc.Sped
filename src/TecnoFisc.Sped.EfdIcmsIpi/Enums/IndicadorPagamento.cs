namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de pagamento — campo IND_PGTO do Registro C100 no EFD ICMS-IPI.
/// Valores vigentes a partir de 01/07/2012 conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 61-62.
/// </summary>
public enum IndicadorPagamento
{
    /// <summary>0 — À vista.</summary>
    AVista = 0,

    /// <summary>1 — A prazo.</summary>
    APrazo = 1,

    /// <summary>2 — Outros.</summary>
    Outros = 2,
}
