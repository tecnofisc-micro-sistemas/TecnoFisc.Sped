namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador da origem do processo judicial ou administrativo no contexto ISS — campo <c>IND_PROC</c>
/// do Registro B460. Valores distintos do homônimo em EFD Contribuições (DF-ISS usa Sefin e
/// Justiça Estadual em vez de Receita Federal).
/// </summary>
public enum IndicadorOrigemProcessoIss
{
    /// <summary>0 — Sefin (Secretaria de Fazenda do Distrito Federal).</summary>
    Sefin = 0,

    /// <summary>1 — Justiça Federal.</summary>
    JusticaFederal = 1,

    /// <summary>2 — Justiça Estadual.</summary>
    JusticaEstadual = 2,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
