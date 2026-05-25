namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de entidade sujeita a auditoria independente — campo IND_GRANDE_PORTE do
/// Registro 0000 da ECD.
/// </summary>
public enum IndicadorGrandePorte
{
    /// <summary>0 — Empresa não é entidade sujeita a auditoria independente.</summary>
    NaoSujeitaAuditoria = 0,

    /// <summary>
    /// 1 — Empresa é entidade sujeita a auditoria independente (Ativo Total superior a
    /// R$ 240.000.000,00 ou Receita Bruta Anual superior a R$ 300.000.000,00).
    /// </summary>
    SujeitaAuditoria = 1,
}
