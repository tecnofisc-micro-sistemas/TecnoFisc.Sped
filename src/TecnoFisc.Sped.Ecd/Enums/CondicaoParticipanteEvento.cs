namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Condição da empresa relacionada ao evento societário — campo <c>COND_PART</c> do Registro K115
/// da ECD. Conforme Manual de Orientação do Leiaute 9 da ECD, p. 218.
/// </summary>
public enum CondicaoParticipanteEvento
{
    /// <summary>1 — Sucessora.</summary>
    Sucessora = 1,

    /// <summary>2 — Adquirente.</summary>
    Adquirente = 2,

    /// <summary>3 — Alienante.</summary>
    Alienante = 3,
}
