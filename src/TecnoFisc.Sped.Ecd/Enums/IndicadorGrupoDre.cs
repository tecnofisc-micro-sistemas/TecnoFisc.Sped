using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de grupo da DRE — campo <c>IND_GRP_DRE</c> do Registro J150 da ECD.
/// D = Despesa (linha com natureza de despesa — redução do lucro);
/// R = Receita (linha com natureza de receita — incremento do lucro).
/// </summary>
public enum IndicadorGrupoDre
{
    /// <summary>D — Linha totalizadora ou de detalhe que, por sua natureza de despesa, representa redução do lucro.</summary>
    [SpedValor("D")]
    Despesa = 0,

    /// <summary>R — Linha totalizadora ou de detalhe que, por sua natureza de receita, representa incremento do lucro.</summary>
    [SpedValor("R")]
    Receita = 1,
}
