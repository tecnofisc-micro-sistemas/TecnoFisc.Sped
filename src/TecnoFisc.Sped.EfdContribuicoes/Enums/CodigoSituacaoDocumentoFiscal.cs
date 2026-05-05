namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código da situação do documento fiscal — campo COD_SIT do Registro A100.
/// Valores conforme Guia Prático v1.35, p. 96.
/// </summary>
public enum CodigoSituacaoDocumentoFiscal
{
    /// <summary>00 — Documento regular.</summary>
    DocumentoRegular = 0,

    /// <summary>02 — Documento cancelado.</summary>
    DocumentoCancelado = 2,
}
