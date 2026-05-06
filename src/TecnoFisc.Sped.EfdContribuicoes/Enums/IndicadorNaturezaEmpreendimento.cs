namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza específica do empreendimento — campo IND_NAT_EMP do Registro F200.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 251.
/// </summary>
public enum IndicadorNaturezaEmpreendimento
{
    /// <summary>1 — Consórcio.</summary>
    Consorcio = 1,

    /// <summary>2 — SCP (Sociedade em Conta de Participação).</summary>
    Scp = 2,

    /// <summary>3 — Incorporação em Condomínio.</summary>
    IncorporacaoCondominio = 3,

    /// <summary>4 — Outras.</summary>
    Outras = 4,
}
