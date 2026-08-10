using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo do fluxo informado no registro Y520.</summary>
public enum TipoOperacaoExterior
{
    /// <summary>R - rendimentos recebidos.</summary>
    [SpedValor("R")]
    RendimentosRecebidos = 0,

    /// <summary>P - pagamentos.</summary>
    [SpedValor("P")]
    Pagamentos = 1,
}
