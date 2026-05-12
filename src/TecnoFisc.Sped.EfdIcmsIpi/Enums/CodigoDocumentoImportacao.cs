namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Código do documento de importação — campo <c>COD_DOC_IMP</c> do Registro C120.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 72.
/// </summary>
public enum CodigoDocumentoImportacao
{
    /// <summary>0 — Declaração de Importação.</summary>
    DeclaracaoImportacao = 0,

    /// <summary>1 — Declaração Simplificada de Importação.</summary>
    DeclaracaoSimplificadaImportacao = 1,
}
