namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza da retenção na fonte — campo IND_NAT_RET do Registro F600.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 278.
/// </summary>
public enum IndicadorNaturezaRetencao
{
    /// <summary>01 — Retenção por Órgãos, Autarquias e Fundações Federais (art. 64 da Lei nº 9.430/96).</summary>
    OrgaosAutarquiasFundacoesFederais = 1,

    /// <summary>02 — Retenção por outras Entidades da Administração Pública Federal (art. 34 da Lei nº 10.833/03).</summary>
    OutrasEntidadesAdmPublicaFederal = 2,

    /// <summary>03 — Retenção por Pessoas Jurídicas de Direito Privado (art. 30 da Lei nº 10.833/03).</summary>
    PessoasJuridicasDireitoPrivado = 3,

    /// <summary>04 — Recolhimento por Sociedade Cooperativa (art. 66 da Lei nº 9.430/96).</summary>
    RecolhimentoSociedadeCooperativa = 4,

    /// <summary>05 — Retenção por Fabricante de Máquinas e Veículos (art. 3º da Lei nº 10.485/02).</summary>
    FabricanteMaquinasVeiculos = 5,

    /// <summary>99 — Outras Retenções.</summary>
    OutrasRetencoes = 99,
}
