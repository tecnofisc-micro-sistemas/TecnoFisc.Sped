namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Código de tipo de ligação elétrica — campo TP_LIGACAO no Registro C500.
/// Obrigatório nas operações de saída quando COD_MOD = "06". Não aplicável para COD_MOD = "66".
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 136.
/// </summary>
public enum TipoLigacaoEletrica
{
    /// <summary>1 — Monofásico.</summary>
    Monofasico = 1,

    /// <summary>2 — Bifásico.</summary>
    Bifasico = 2,

    /// <summary>3 — Trifásico.</summary>
    Trifasico = 3,
}
