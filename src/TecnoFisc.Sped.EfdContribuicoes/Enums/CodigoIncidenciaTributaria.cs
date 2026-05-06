namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código indicador da incidência tributária no período — campo COD_INC_TRIB do Registro 0110.
/// Valores válidos pelo Guia Prático v1.35, p. 72: <c>1, 2, 3</c>.
/// </summary>
public enum CodigoIncidenciaTributaria
{
    /// <summary>1 — Escrituração de operações com incidência exclusivamente no regime não-cumulativo.</summary>
    RegimeNaoCumulativo = 1,

    /// <summary>2 — Escrituração de operações com incidência exclusivamente no regime cumulativo.</summary>
    RegimeCumulativo = 2,

    /// <summary>3 — Escrituração de operações com incidência nos regimes não-cumulativo e cumulativo.</summary>
    RegimesNaoCumulativoECumulativo = 3,
}
