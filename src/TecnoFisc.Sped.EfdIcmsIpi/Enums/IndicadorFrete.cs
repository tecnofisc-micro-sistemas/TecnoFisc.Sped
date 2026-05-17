namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de frete/transporte — campo IND_FRT nos registros de escrituração de
/// documentos fiscais do EFD ICMS-IPI (ex.: C100, D100).
/// Valores vigentes a partir de 01/01/2018 conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 61-63.
/// </summary>
public enum IndicadorFrete
{
    /// <summary>0 — Contratação do Frete por conta do Remetente (CIF).</summary>
    ContaRemetenteCif = 0,

    /// <summary>1 — Contratação do Frete por conta do Destinatário (FOB).</summary>
    ContaDestinatarioFob = 1,

    /// <summary>2 — Contratação do Frete por conta de Terceiros.</summary>
    ContaTerceiros = 2,

    /// <summary>3 — Transporte Próprio por conta do Remetente.</summary>
    TransporteProprioRemetente = 3,

    /// <summary>4 — Transporte Próprio por conta do Destinatário.</summary>
    TransporteProprioDestinatario = 4,

    /// <summary>9 — Sem Ocorrência de Transporte.</summary>
    SemTransporte = 9,
}
