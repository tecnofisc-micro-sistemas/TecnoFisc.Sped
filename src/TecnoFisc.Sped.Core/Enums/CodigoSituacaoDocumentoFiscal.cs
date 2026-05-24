using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Código da situação do documento fiscal — Tabela 4.1.2 do Ato COTEPE/ICMS nº 09/2008
/// e nº 44/2018. Regida pelo EFD ICMS-IPI; referenciada por todos os leiautes SPED
/// que escrituram documentos fiscais (campo COD_SIT).
/// </summary>
/// <remarks>
/// Códigos 04 (NF-e/NFC-e/CT-e denegado) e 05 (NF-e/NFC-e/CT-e numeração inutilizada)
/// descontinuados a partir de janeiro de 2023 — mantidos no enum apenas para parsing
/// de arquivos com fatos geradores anteriores ao corte.
/// </remarks>
public enum CodigoSituacaoDocumentoFiscal
{
    /// <summary>00 — Documento regular.</summary>
    DocumentoRegular = 0,

    /// <summary>01 — Documento regular (extemporâneo).</summary>
    DocumentoRegularExtemporaneo = 1,

    /// <summary>02 — Documento cancelado.</summary>
    DocumentoCancelado = 2,

    /// <summary>03 — Documento cancelado (extemporâneo).</summary>
    DocumentoCanceladoExtemporaneo = 3,

    /// <summary>
    /// 04 — NF-e denegada (somente para emissão própria). Descontinuado em V017 do EFD
    /// ICMS-IPI (Guia Prático 3.1.0 item 1, vigente a partir de janeiro/2023). Mantido
    /// no enum para parsing de arquivos históricos — pacote é read-only (ARCHITECTURE
    /// §4.7) e continua reconhecendo o código.
    /// </summary>
    // EmVersao = 17 corresponde a LayoutEfdIcmsIpi.V017 (COD_VER do registro 0000). Usa-se
    // literal porque Core não pode depender de EfdIcmsIpi (Hard Rule 2 — sem referência
    // entre projetos format-specific). Enum-of-versão regente fica no módulo do leiaute.
    [Descontinuado(EmVersao = 17)]
    NfeDenegada = 4,

    /// <summary>
    /// 05 — NF-e com numeração inutilizada (somente para emissão própria). Descontinuado
    /// em V017 do EFD ICMS-IPI (Guia Prático 3.1.0 item 1, vigente a partir de
    /// janeiro/2023). Mantido no enum para parsing de arquivos históricos — pacote é
    /// read-only (ARCHITECTURE §4.7) e continua reconhecendo o código.
    /// </summary>
    // EmVersao = 17 corresponde a LayoutEfdIcmsIpi.V017 (COD_VER do registro 0000). Usa-se
    // literal porque Core não pode depender de EfdIcmsIpi (Hard Rule 2 — sem referência
    // entre projetos format-specific). Enum-of-versão regente fica no módulo do leiaute.
    [Descontinuado(EmVersao = 17)]
    NumeracaoInutilizada = 5,

    /// <summary>06 — Documento Fiscal Complementar.</summary>
    DocumentoFiscalComplementar = 6,

    /// <summary>07 — Documento Fiscal Complementar (extemporâneo).</summary>
    DocumentoFiscalComplementarExtemporaneo = 7,

    /// <summary>08 — Documento Fiscal emitido com base em Regime Especial ou Norma Específica.</summary>
    RegimeEspecial = 8,
}
