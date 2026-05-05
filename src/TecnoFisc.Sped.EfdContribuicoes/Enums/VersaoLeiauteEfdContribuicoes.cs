namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Versão do leiaute da EFD Contribuições — Tabela 3.1.1. Campo COD_VER do Registro 0000.
/// </summary>
/// <remarks>
/// O código 002 corresponde a duas versões distintas do leiaute (1.01 instituída pelos
/// ADE Cofis nº 34/2010 e nº 37/2010, e 2.00 instituída pelo ADE Cofis nº 20/2012),
/// ambas com período de apuração inicial 2011-04-01. O enum mapeia somente o código,
/// não diferencia entre as duas — o consumidor deve identificar a versão semântica
/// pelo período de apuração.
/// </remarks>
public enum VersaoLeiauteEfdContribuicoes
{
    /// <summary>Código 001 — Leiaute v1.00 (ADE Cofis nº 31/2010). Vigência: 2011-04-01.</summary>
    V001 = 1,

    /// <summary>Código 002 — Leiaute v1.01 (ADE Cofis nº 34/2010 e nº 37/2010) ou v2.00 (ADE Cofis nº 20/2012). Vigência: 2011-04-01.</summary>
    V002 = 2,

    /// <summary>Código 003 — Leiaute v2.01A (ADE Cofis nº 20/2012). Vigência: 2012-07-01.</summary>
    V003 = 3,

    /// <summary>Código 004 — Leiaute v3.0.0 (ADE Cofis nº 20/2012). Vigência: 2018-06-01.</summary>
    V004 = 4,

    /// <summary>Código 005 — Leiaute v3.1.0 (ADE Cofis nº 82/2018). Vigência: 2019-01-01.</summary>
    V005 = 5,

    /// <summary>Código 006 — Leiaute v3.2.0. Vigência: 2020-01-01.</summary>
    V006 = 6,
}
