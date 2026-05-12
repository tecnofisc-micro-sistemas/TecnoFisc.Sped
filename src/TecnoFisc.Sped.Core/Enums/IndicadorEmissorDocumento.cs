namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Indicador do emitente do documento fiscal — campo IND_EMIT em registros de
/// escrituração de documentos (ex.: B020 no EFD ICMS-IPI, C100 no EFD Contribuições).
/// EFD ICMS-IPI é o regente; valor vive no Core para reuso transversal.
/// </summary>
public enum IndicadorEmissorDocumento
{
    /// <summary>0 — Emissão própria.</summary>
    EmissaoPropria = 0,

    /// <summary>1 — Emissão por terceiros.</summary>
    Terceiros = 1,
}
