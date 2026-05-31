namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de atividade para fins de IPI — campo IND_ATIV do Registro 0000
/// do EFD ICMS-IPI. Classifica o estabelecimento conforme legislação do IPI
/// (Decreto nº 7.212/2010). IPI é tributo ICMS-IPI domain; enum vive no Core para reuso.
/// </summary>
public enum IndicadorAtividadeIpi
{
    /// <summary>0 — Industrial ou equiparado a industrial.</summary>
    Industrial = 0,

    /// <summary>1 — Outros.</summary>
    Outros = 1,
}
