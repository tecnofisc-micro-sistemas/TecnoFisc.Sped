using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Natureza do controle ou relacionamento da participação no exterior.</summary>
public enum TipoControleExterior
{
    [SpedValor("1")]
    ControladaDireta = 1,

    [SpedValor("2")]
    ControladaIndireta = 2,

    [SpedValor("3")]
    EquiparadaControlada = 3,

    [SpedValor("4")]
    ColigadaCompetencia = 4,

    [SpedValor("5")]
    FilialOuSucursal = 5,

    [SpedValor("6")]
    ColigadaCaixa = 6,

    [SpedValor("7")]
    JointVenture = 7,

    [SpedValor("8")]
    Partnership = 8,

    [SpedValor("9")]
    Trust = 9,

    [SpedValor("10")]
    ColigadaCompetenciaOpcao = 10,

    [SpedValor("11")]
    ColigadaRegimeMisto = 11,
}
