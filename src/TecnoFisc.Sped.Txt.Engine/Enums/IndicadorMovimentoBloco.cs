namespace TecnoFisc.Sped.Txt.Engine.Enums;

/// <summary>
/// Indicador de movimento de um bloco — campo IND_MOV dos registros de abertura (X001).
/// Transversal a todos os leiautes SPED (EFD ICMS-IPI, EFD Contribuições, etc.).
/// Regido pelo Ato COTEPE/ICMS nº 44/2018; EFD ICMS-IPI é o regente.
/// </summary>
public enum IndicadorMovimentoBloco
{
    /// <summary>0 — Bloco contém registros de movimento além das aberturas/encerramentos.</summary>
    ComDados = 0,

    /// <summary>1 — Bloco sem registros de movimento — apenas abertura e encerramento.</summary>
    SemDados = 1,
}
