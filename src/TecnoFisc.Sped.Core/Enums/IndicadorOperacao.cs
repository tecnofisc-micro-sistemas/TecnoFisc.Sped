namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Indicador do tipo de operação do documento fiscal — campo IND_OPER em registros de
/// escrituração de documentos (ex.: C100, D100 no EFD ICMS-IPI).
/// Transversal a todos os leiautes SPED que registram entradas e saídas.
/// EFD ICMS-IPI é o regente; valor vive no Core para reuso transversal.
/// </summary>
public enum IndicadorOperacao
{
    /// <summary>0 — Entrada.</summary>
    Entrada = 0,

    /// <summary>1 — Saída.</summary>
    Saida = 1,
}
