namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de documento de exportacao — campo IND_DOC do Registro 1100.
/// Valores conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 269.
/// </summary>
public enum IndicadorDocumentoExportacao
{
    /// <summary>0 - Declaracao de Exportacao.</summary>
    DeclaracaoExportacao = 0,

    /// <summary>1 - Declaracao Simplificada de Exportacao.</summary>
    DeclaracaoSimplificadaExportacao = 1,

    /// <summary>2 - Declaracao Unica de Exportacao.</summary>
    DeclaracaoUnicaExportacao = 2,
}
