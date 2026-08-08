using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Condição do declarante no consórcio informada no registro Y640.</summary>
public enum CondicaoDeclaranteConsorcio
{
    /// <summary>1 - líder.</summary>
    [SpedValor("1")]
    Lider = 1,

    /// <summary>2 - participante.</summary>
    [SpedValor("2")]
    Participante = 2,
}
