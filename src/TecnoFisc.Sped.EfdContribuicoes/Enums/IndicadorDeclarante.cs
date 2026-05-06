namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da condição da pessoa jurídica declarante do Registro F600 — campo IND_DEC.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 280.
/// </summary>
public enum IndicadorDeclarante
{
    /// <summary>0 — Beneficiária da Retenção / Recolhimento.</summary>
    BeneficiariaDaRetencao = 0,

    /// <summary>1 — Responsável pelo Recolhimento (sociedade cooperativa).</summary>
    ResponsavelPeloRecolhimento = 1,
}
