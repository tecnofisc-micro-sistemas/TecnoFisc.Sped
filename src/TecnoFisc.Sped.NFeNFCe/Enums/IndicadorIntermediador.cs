namespace TecnoFisc.Sped.NFeNFCe.Enums;

/// <summary>Indicador de intermediador/marketplace — campo indIntermed do grupo ide.</summary>
public enum IndicadorIntermediador
{
    /// <summary>0 — Operação sem intermediador (venda direta).</summary>
    SemIntermediador = 0,

    /// <summary>1 — Operação em site ou plataforma de terceiros.</summary>
    ComIntermediador = 1,
}
