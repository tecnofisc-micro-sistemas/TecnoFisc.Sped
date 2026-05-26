namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador do tipo de ECD — campo TIP_ECD do Registro 0000. Distingue a escrituração
/// quanto à participação em Sociedade em Conta de Participação (SCP).
/// </summary>
public enum TipoEcd
{
    /// <summary>0 — ECD de empresa não participante de SCP como sócio ostensivo.</summary>
    NaoParticipanteScp = 0,

    /// <summary>1 — ECD de empresa participante de SCP como sócio ostensivo.</summary>
    ParticipanteScpOstensivo = 1,

    /// <summary>2 — ECD da SCP.</summary>
    Scp = 2,
}
