using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Tipo de faturamento da NFCom — campo TIP_FAT no Registro D700 (NFCom, código 62).
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 213-215 (Subseção 12).
/// </summary>
public enum TipoFaturamentoNFCom
{
    /// <summary>0 — Faturamento Normal.</summary>
    [SpedValor("0")]
    Normal = 0,

    /// <summary>1 — Faturamento centralizado.</summary>
    [SpedValor("1")]
    Centralizado = 1,

    /// <summary>2 — Cofaturamento.</summary>
    [SpedValor("2")]
    Cofaturamento = 2,
}
