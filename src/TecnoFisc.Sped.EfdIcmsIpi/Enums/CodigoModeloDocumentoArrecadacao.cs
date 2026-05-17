namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Código do modelo do documento de arrecadação — campo COD_DA do Registro C112.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 68.
/// </summary>
public enum CodigoModeloDocumentoArrecadacao
{
    /// <summary>0 — Documento estadual de arrecadação.</summary>
    DocumentoEstadual = 0,

    /// <summary>1 — GNRE (Guia Nacional de Recolhimento de Tributos Estaduais).</summary>
    Gnre = 1,
}
