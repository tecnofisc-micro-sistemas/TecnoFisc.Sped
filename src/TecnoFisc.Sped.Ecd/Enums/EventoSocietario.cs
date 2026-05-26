namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Evento societário ocorrido no período de consolidação — campo <c>EVENTO</c> do Registro K110
/// da ECD. Conforme Manual de Orientação do Leiaute 9 da ECD, p. 216.
/// </summary>
public enum EventoSocietario
{
    /// <summary>1 — Aquisição.</summary>
    Aquisicao = 1,

    /// <summary>2 — Alienação.</summary>
    Alienacao = 2,

    /// <summary>3 — Fusão.</summary>
    Fusao = 3,

    /// <summary>4 — Cisão Parcial.</summary>
    CisaoParcial = 4,

    /// <summary>5 — Cisão Total.</summary>
    CisaoTotal = 5,

    /// <summary>6 — Incorporação.</summary>
    Incorporacao = 6,

    /// <summary>7 — Extinção.</summary>
    Extincao = 7,

    /// <summary>8 — Constituição.</summary>
    Constituicao = 8,
}
