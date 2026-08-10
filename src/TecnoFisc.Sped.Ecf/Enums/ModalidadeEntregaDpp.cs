using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Modalidade de entrega da Declaração País-a-País.</summary>
public enum ModalidadeEntregaDpp
{
    /// <summary>1 - entidade substituta.</summary>
    [SpedValor("1")]
    EntidadeSubstituta = 1,

    /// <summary>2 - preenchimento local.</summary>
    [SpedValor("2")]
    PreenchimentoLocal = 2,
}
