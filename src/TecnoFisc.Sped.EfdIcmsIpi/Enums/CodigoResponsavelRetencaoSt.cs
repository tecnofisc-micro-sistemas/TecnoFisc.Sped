namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Código do responsável pela retenção do ICMS ST — campo COD_RESP_RET do Registro C176.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 86.
/// </summary>
public enum CodigoResponsavelRetencaoSt
{
    /// <summary>1 — Remetente Direto.</summary>
    RemetenteDireto = 1,

    /// <summary>2 — Remetente Indireto.</summary>
    RemetenteIndireto = 2,

    /// <summary>3 — Próprio declarante.</summary>
    ProprioDeclarante = 3,
}
