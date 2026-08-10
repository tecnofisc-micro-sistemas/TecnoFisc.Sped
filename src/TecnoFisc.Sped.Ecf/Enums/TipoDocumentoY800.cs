using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo do documento RTF anexado pelo registro Y800.</summary>
public enum TipoDocumentoY800
{
    /// <summary>001 - memória de cálculo de incorporação.</summary>
    [SpedValor("001")]
    MemoriaCalculoIncorporacao = 1,

    /// <summary>002 - laudo de avaliação a valor justo.</summary>
    [SpedValor("002")]
    LaudoAvaliacaoValorJusto = 2,

    /// <summary>003 - outros documentos.</summary>
    [SpedValor("003")]
    Outros = 3,
}
