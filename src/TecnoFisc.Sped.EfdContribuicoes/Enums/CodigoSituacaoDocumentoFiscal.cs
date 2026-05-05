namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código da situação do documento fiscal — campo COD_SIT dos Registros A100 e C100.
/// Tabela 4.1.2. Valores conforme Guia Prático v1.35, p. 96/108.
/// </summary>
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

    /// <summary>04 — NF-e denegada (somente para emissão própria).</summary>
    NfeDenegada = 4,

    /// <summary>05 — NF-e com numeração inutilizada (somente para emissão própria).</summary>
    NumeracaoInutilizada = 5,

    /// <summary>06 — Documento Fiscal Complementar.</summary>
    DocumentoFiscalComplementar = 6,

    /// <summary>07 — Documento Fiscal Complementar (extemporâneo).</summary>
    DocumentoFiscalComplementarExtemporaneo = 7,

    /// <summary>08 — Documento Fiscal emitido com base em Regime Especial ou Norma Específica.</summary>
    RegimeEspecial = 8,
}
