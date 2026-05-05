namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do emitente do documento fiscal — campo IND_EMIT do Registro A100.
/// Valores conforme Guia Prático v1.35, p. 96.
/// </summary>
public enum IndicadorEmissaoDocumento
{
    /// <summary>0 — Emissão Própria.</summary>
    EmissaoPropria = 0,

    /// <summary>1 — Emissão de Terceiros.</summary>
    EmissaoPorTerceiros = 1,
}
