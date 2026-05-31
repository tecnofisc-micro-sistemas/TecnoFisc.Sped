using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Forma de pagamento na escrituração consolidada NFCom — campo IND_PREPAGO no Registro D750
/// (código 62). Identifica se o documento consolidado representa serviços pré-pagos ou pós-pagos.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 219 (Subseção 12).
/// </summary>
public enum FormaPagamentoConsolidadaNFCom
{
    /// <summary>0 — Pré-pago.</summary>
    [SpedValor("0")]
    PrePago = 0,

    /// <summary>1 — Pós-pago.</summary>
    [SpedValor("1")]
    PosPago = 1,
}
