namespace TecnoFisc.Sped.Core.Enums;

/// <summary>Modalidade do frete — campo modFrete do grupo transp.</summary>
public enum ModalidadeFrete
{
    /// <summary>0 — Por conta do emitente (CIF).</summary>
    PorContaEmitente = 0,

    /// <summary>1 — Por conta do destinatário/remetente (FOB).</summary>
    PorContaDestinatario = 1,

    /// <summary>2 — Por conta de terceiros.</summary>
    PorContaTerceiros = 2,

    /// <summary>3 — Transporte próprio por conta do remetente.</summary>
    ProprioRemetente = 3,

    /// <summary>4 — Transporte próprio por conta do destinatário.</summary>
    ProprioDestinatario = 4,

    /// <summary>9 — Sem ocorrência de transporte.</summary>
    SemTransporte = 9,
}
