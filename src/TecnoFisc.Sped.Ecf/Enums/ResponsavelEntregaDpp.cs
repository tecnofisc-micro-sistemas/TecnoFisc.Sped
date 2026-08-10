using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Entidade responsável pela entrega da Declaração País-a-País.</summary>
public enum ResponsavelEntregaDpp
{
    /// <summary>1 - grupo multinacional dispensado da entrega.</summary>
    [SpedValor("1")]
    GrupoDispensado = 1,

    /// <summary>2 - controlador final do grupo multinacional.</summary>
    [SpedValor("2")]
    ControladorFinal = 2,

    /// <summary>3 - própria entidade declarante da ECF.</summary>
    [SpedValor("3")]
    PropriaEntidade = 3,

    /// <summary>4 - outra entidade integrante.</summary>
    [SpedValor("4")]
    OutraEntidade = 4,
}
