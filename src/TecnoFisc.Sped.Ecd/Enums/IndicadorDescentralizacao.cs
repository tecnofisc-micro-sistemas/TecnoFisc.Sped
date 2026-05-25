namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de descentralização da escrituração contábil — campo IND_DEC do Registro 0020 da ECD.
/// </summary>
public enum IndicadorDescentralizacao
{
    /// <summary>0 — Escrituração da Matriz.</summary>
    EscrituradorMatriz = 0,

    /// <summary>1 — Escrituração da Filial.</summary>
    EscrituradorFilial = 1,
}
