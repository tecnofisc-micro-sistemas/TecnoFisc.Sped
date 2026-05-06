namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do número de parcelas a serem apropriadas (crédito sobre valor de aquisição) —
/// campo IND_NR_PARC do Registro F130.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 242.
/// </summary>
public enum IndicadorNumeroParcelas
{
    /// <summary>1 — Integral (Mês de Aquisição).</summary>
    Integral = 1,

    /// <summary>2 — 12 Meses.</summary>
    DozeMeses = 2,

    /// <summary>3 — 24 Meses.</summary>
    VinteQuatroMeses = 3,

    /// <summary>4 — 48 Meses.</summary>
    QuarentaOitoMeses = 4,

    /// <summary>5 — 6 Meses (Embalagens de bebidas frias).</summary>
    SeisMeses = 5,

    /// <summary>9 — Outra periodicidade definida em Lei.</summary>
    OutraPeriodicidade = 9,
}
