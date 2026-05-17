namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Código da finalidade do arquivo — campo COD_FIN do Registro 0000 do EFD ICMS-IPI.
/// Regido pelo Ato COTEPE/ICMS nº 44/2018; EFD ICMS-IPI é o regente.
/// </summary>
public enum IndicadorFinalidadeArquivo
{
    /// <summary>0 — Remessa do arquivo original.</summary>
    Original = 0,

    /// <summary>1 — Remessa do arquivo substituto.</summary>
    Substituto = 1,
}
