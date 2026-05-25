namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Identificação das demonstrações contábeis — campo <c>ID_DEM</c> do Registro C600 da ECD.
/// 1 = Demonstrações contábeis do empresário ou sociedade empresária a que se refere a escrituração;
/// 2 = Demonstrações consolidadas ou de outras pessoas jurídicas.
/// </summary>
public enum IdentificacaoDemonstracao
{
    /// <summary>1 — Demonstrações contábeis do empresário ou sociedade empresária.</summary>
    Empresario = 1,

    /// <summary>2 — Demonstrações consolidadas ou de outras pessoas jurídicas.</summary>
    Consolidado = 2,
}
