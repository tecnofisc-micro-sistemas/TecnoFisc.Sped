using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo de endereço da entidade integrante na Declaração País-a-País.</summary>
public enum TipoEnderecoDpp
{
    /// <summary>OECD301.</summary>
    [SpedValor("OECD301")]
    Oecd301 = 0,

    /// <summary>OECD302 - residencial.</summary>
    [SpedValor("OECD302")]
    Oecd302Residencial = 1,

    /// <summary>OECD303 - comercial.</summary>
    [SpedValor("OECD303")]
    Oecd303Comercial = 2,

    /// <summary>OECD304.</summary>
    [SpedValor("OECD304")]
    Oecd304 = 3,

    /// <summary>OECD305.</summary>
    [SpedValor("OECD305")]
    Oecd305 = 4,
}
