using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Finalidade da emissão do documento eletrônico — campo FIN_DOCe no Registro D700 (NFCom, código 62).
/// Valores específicos da NFCom (0, 3, 4) — distintos dos valores 1/2/3 usados por outros documentos
/// eletrônicos (ver <see cref="FinalidadeDocumentoEletronico"/>). Enum separado por divergência semântica.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 213-215 (Subseção 12).
/// </summary>
public enum FinalidadeDocumentoEletronicoNFCom
{
    /// <summary>0 — NFCom Normal.</summary>
    [SpedValor("0")]
    Normal = 0,

    /// <summary>3 — NFCom de Substituição.</summary>
    [SpedValor("3")]
    Substituicao = 3,

    /// <summary>4 — NFCom de Ajuste.</summary>
    [SpedValor("4")]
    Ajuste = 4,
}
