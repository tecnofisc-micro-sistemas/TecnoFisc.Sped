namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador da obrigação tributária do ISS à qual a dedução será aplicada — campo <c>IND_OBR</c>
/// do Registro B460. Exclusivo do Bloco B (ISS); não regido pelo Ato COTEPE/ICMS.
/// </summary>
public enum IndicadorObrigacaoIss
{
    /// <summary>0 — ISS Próprio.</summary>
    IssProprio = 0,

    /// <summary>1 — ISS Substituto (devido pelas aquisições de serviços do declarante).</summary>
    IssSubstituto = 1,

    /// <summary>2 — ISS Uniprofissional.</summary>
    IssUniprofissional = 2,
}
