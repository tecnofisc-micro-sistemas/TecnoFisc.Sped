namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Indicador do tipo de ajuste da apuração do IPI — campo <c>IND_AJ</c> do Registro E530.
/// IPI é tributo do domínio ICMS-IPI; o enum vive no Core para reuso transversal.
/// </summary>
public enum IndicadorTipoAjusteIpi
{
    /// <summary>0 — Ajuste a débito.</summary>
    Debito = 0,

    /// <summary>1 — Ajuste a crédito.</summary>
    Credito = 1,
}
