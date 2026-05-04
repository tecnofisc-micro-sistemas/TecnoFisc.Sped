namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código indicador da incidência da Contribuição Previdenciária sobre a Receita Bruta (CPRB)
/// no período — campo COD_INC_TRIB do Registro 0145.
/// Valores válidos pelo Guia Prático v1.35, p. 80: <c>1, 2</c>.
/// </summary>
public enum CodigoIncidenciaContribPrevidenciaria
{
    /// <summary>1 — CP apurada no período exclusivamente com base na Receita Bruta.</summary>
    ReceitaBrutaExclusivamente = 1,

    /// <summary>
    /// 2 — CP apurada com base na Receita Bruta e com base nas Remunerações pagas,
    /// na forma dos incisos I e III do art. 22 da Lei nº 8.212/1991.
    /// </summary>
    ReceitaBrutaERemuneracoes = 2,
}
