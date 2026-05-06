namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código do documento de importação — campo COD_DOC_IMP do Registro C120.
/// Valores vigentes a partir de 01/2019 (Guia Prático v1.35, p. 116).
/// </summary>
public enum CodigoDocumentoImportacao
{
    /// <summary>0 — Declaração de Importação.</summary>
    DeclaracaoImportacao = 0,

    /// <summary>1 — Declaração Simplificada de Importação.</summary>
    DeclaracaoSimplificadaImportacao = 1,

    /// <summary>2 — Declaração Única de Importação.</summary>
    DeclaracaoUnicaImportacao = 2,
}
