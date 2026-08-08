using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo de processo informado em um lançamento fiscal.</summary>
public enum TipoProcessoEcf
{
    /// <summary>1 - judicial.</summary>
    [SpedValor("1")]
    Judicial = 0,

    /// <summary>2 - administrativo.</summary>
    [SpedValor("2")]
    Administrativo = 1,
}
