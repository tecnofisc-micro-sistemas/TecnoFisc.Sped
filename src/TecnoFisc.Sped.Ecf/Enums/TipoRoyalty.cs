using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Natureza do royalty informado no registro X420.</summary>
public enum TipoRoyalty
{
    /// <summary>R - royalty recebido.</summary>
    [SpedValor("R")]
    Recebido = 0,

    /// <summary>P - royalty pago.</summary>
    [SpedValor("P")]
    Pago = 1,
}
