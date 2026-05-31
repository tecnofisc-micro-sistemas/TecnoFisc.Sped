using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de grupo do balanço — campo <c>IND_GRP_BAL</c> do Registro J100 da ECD.
/// A = Ativo; P = Passivo e Patrimônio Líquido.
/// </summary>
public enum IndicadorGrupoBalanco
{
    /// <summary>A — Ativo.</summary>
    [SpedValor("A")]
    Ativo = 0,

    /// <summary>P — Passivo e Patrimônio Líquido.</summary>
    [SpedValor("P")]
    PassivoPatrimonioLiquido = 1,
}
