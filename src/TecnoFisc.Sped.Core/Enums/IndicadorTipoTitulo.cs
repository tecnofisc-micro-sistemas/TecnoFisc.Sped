namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Indicador do tipo de título de crédito — campo IND_TIT no Registro C140 (EFD ICMS-IPI).
/// </summary>
public enum IndicadorTipoTitulo
{
    /// <summary>00 — Duplicata.</summary>
    Duplicata = 0,

    /// <summary>01 — Cheque.</summary>
    Cheque = 1,

    /// <summary>02 — Promissória.</summary>
    Promissoria = 2,

    /// <summary>03 — Recibo.</summary>
    Recibo = 3,

    /// <summary>99 — Outros (descrever no campo DESC_TIT).</summary>
    Outros = 99,
}
