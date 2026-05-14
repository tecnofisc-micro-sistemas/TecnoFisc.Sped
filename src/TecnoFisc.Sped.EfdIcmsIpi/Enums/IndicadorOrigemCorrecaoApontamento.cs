namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador da origem da correção de apontamento — campo ORIGEM no Registro K270.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 260-261.
/// </summary>
public enum IndicadorOrigemCorrecaoApontamento
{
    /// <summary>1 — Correção de produção e/ou consumo dos Registros K230/K235.</summary>
    ProducaoConsumoK230K235 = 1,

    /// <summary>2 — Correção de produção e/ou consumo dos Registros K250/K255.</summary>
    ProducaoConsumoK250K255 = 2,

    /// <summary>3 — Correção de desmontagem e/ou consumo dos Registros K210/K215.</summary>
    DesmontagemConsumoK210K215 = 3,

    /// <summary>4 — Correção de reprocessamento/reparo e/ou consumo dos Registros K260/K265.</summary>
    ReprocessamentoReparoConsumoK260K265 = 4,

    /// <summary>5 — Correção de movimentação interna do Registro K220.</summary>
    MovimentacaoInternaK220 = 5,

    /// <summary>6 — Correção de produção do Registro K291.</summary>
    ProducaoK291 = 6,

    /// <summary>7 — Correção de consumo do Registro K292.</summary>
    ConsumoK292 = 7,

    /// <summary>8 — Correção de produção do Registro K301.</summary>
    ProducaoK301 = 8,

    /// <summary>9 — Correção de consumo do Registro K302.</summary>
    ConsumoK302 = 9,
}
