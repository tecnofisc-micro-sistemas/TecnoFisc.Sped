namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código de Detalhamento (Registro Filho P110) — Tabela 5.1.2 do Guia Prático EFD
/// Contribuições. Identifica a informação a ser objeto de detalhamento no Registro
/// Filho P110, referente a informações prestadas de forma agregada ou totalizada
/// em campos do Registro P100.
/// </summary>
/// <remarks>
/// Os códigos têm 8 dígitos quando serializados no SPED (ex.: "00000001" para
/// Documento Fiscal). O valor inteiro do enum corresponde ao número significativo
/// do código; a serialização para o leiaute deve aplicar zero-padding para 8 posições.
/// </remarks>
public enum CodigoDetalhamentoCprb
{
    /// <summary>00000001 — Detalhamento por documento fiscal.</summary>
    DocumentoFiscal = 1,

    /// <summary>00000002 — Detalhamento por item/produto/serviço.</summary>
    ItemProdutoServico = 2,

    /// <summary>00000003 — Detalhamento por NCM.</summary>
    Ncm = 3,

    /// <summary>00000004 — Detalhamento por Cliente.</summary>
    Cliente = 4,

    /// <summary>00000999 — Detalhamento por outros critérios.</summary>
    OutrosCriterios = 999,
}
