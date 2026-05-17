namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de dedução do ISS — campo <c>IND_DED</c> do Registro B460.
/// Exclusivo do Bloco B (ISS); não regido pelo Ato COTEPE/ICMS.
/// </summary>
public enum IndicadorDeducaoIss
{
    /// <summary>0 — Compensação do ISS calculado a maior.</summary>
    Compensacao = 0,

    /// <summary>1 — Benefício fiscal por incentivo à cultura.</summary>
    BeneficioFiscalCultura = 1,

    /// <summary>2 — Decisão administrativa ou judicial.</summary>
    DecisaoAdministrativaOuJudicial = 2,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
