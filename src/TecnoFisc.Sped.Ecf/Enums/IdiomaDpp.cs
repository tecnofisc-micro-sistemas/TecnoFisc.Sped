using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Idioma do texto livre da Declaração País-a-País.</summary>
public enum IdiomaDpp
{
    /// <summary>PT - português.</summary>
    [SpedValor("PT")]
    Portugues = 0,

    /// <summary>EN - inglês.</summary>
    [SpedValor("EN")]
    Ingles = 1,

    /// <summary>ES - espanhol.</summary>
    [SpedValor("ES")]
    Espanhol = 2,
}
