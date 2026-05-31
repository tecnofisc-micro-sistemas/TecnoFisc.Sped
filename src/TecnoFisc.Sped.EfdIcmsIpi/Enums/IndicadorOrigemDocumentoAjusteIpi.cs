namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador da origem do documento vinculado ao ajuste de IPI — campo <c>IND_DOC</c> do
/// Registro E530. IPI é tributo do domínio ICMS-IPI; o enum vive no Core para reuso transversal.
/// </summary>
public enum IndicadorOrigemDocumentoAjusteIpi
{
    /// <summary>0 — Processo Judicial.</summary>
    ProcessoJudicial = 0,

    /// <summary>1 — Processo Administrativo.</summary>
    ProcessoAdministrativo = 1,

    /// <summary>2 — PER/DCOMP.</summary>
    PerDcomp = 2,

    /// <summary>3 — Documento Fiscal.</summary>
    DocumentoFiscal = 3,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
