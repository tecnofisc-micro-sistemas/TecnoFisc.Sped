namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador de participação societária do profissional — campo <c>IND_SOC</c> do Registro B510.
/// Exclusivo do Bloco B (ISS Uniprofissional); não regido pelo Ato COTEPE/ICMS.
/// </summary>
public enum IndicadorParticipacaoSocietaria
{
    /// <summary>0 — Sócio.</summary>
    Socio = 0,

    /// <summary>1 — Não sócio.</summary>
    NaoSocio = 1,
}
