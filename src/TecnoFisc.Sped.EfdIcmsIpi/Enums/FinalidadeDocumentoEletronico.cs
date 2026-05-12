namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Finalidade da emissão do documento eletrônico — campo FIN_DOCe no Registro C500.
/// Preenchido somente quando COD_MOD = "66" (NF3e). Para demais modelos, não deve ser informado.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 136-137.
/// </summary>
public enum FinalidadeDocumentoEletronico
{
    /// <summary>1 — Normal.</summary>
    Normal = 1,

    /// <summary>2 — Substituição.</summary>
    Substituicao = 2,

    /// <summary>3 — Normal com ajuste.</summary>
    NormalComAjuste = 3,
}
