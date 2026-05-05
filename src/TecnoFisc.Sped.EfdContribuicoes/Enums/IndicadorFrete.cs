namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de frete/transporte — campo IND_FRT do Registro C100.
/// Valores conforme layout vigente a partir de 01/10/2017. Guia Prático v1.35, p. 108-111.
/// </summary>
public enum IndicadorFrete
{
    /// <summary>0 — Frete por conta do remetente (CIF).</summary>
    ContaRemetenteCif = 0,

    /// <summary>1 — Frete por conta do destinatário (FOB).</summary>
    ContaDestinatarioFob = 1,

    /// <summary>2 — Frete por conta de terceiros.</summary>
    ContaTerceiros = 2,

    /// <summary>3 — Transporte próprio por conta do remetente.</summary>
    TransporteProprioRemetente = 3,

    /// <summary>4 — Transporte próprio por conta do destinatário.</summary>
    TransporteProprioDestinatario = 4,

    /// <summary>9 — Sem ocorrência de transporte.</summary>
    SemTransporte = 9,
}
