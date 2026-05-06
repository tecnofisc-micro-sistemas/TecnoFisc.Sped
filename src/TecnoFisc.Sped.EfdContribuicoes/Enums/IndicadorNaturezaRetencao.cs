namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza da retenção na fonte — campo IND_NAT_RET dos Registros F600, 1300 e 1700.
/// Valores 01–05 e 99 válidos para regime não cumulativo e cumulativo geral. Valores 51–59
/// aplicáveis a partir de 2014 para rendimentos sujeitos ao regime cumulativo auferido por PJ
/// tributada pelo Lucro Real (art. 8º da Lei nº 10.637/2002 e art. 10 da Lei nº 10.833/2003).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 278 e p. 394.
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

    /// <summary>51 — Retenção por Órgãos, Autarquias e Fundações Federais — Regime Cumulativo (PJ Lucro Real).</summary>
    OrgaosAutarquiasFundacoesFederaisCumulativoLucroReal = 51,

    /// <summary>52 — Retenção por outras Entidades da Administração Pública Federal — Regime Cumulativo (PJ Lucro Real).</summary>
    OutrasEntidadesAdmPublicaFederalCumulativoLucroReal = 52,

    /// <summary>53 — Retenção por Pessoas Jurídicas de Direito Privado — Regime Cumulativo (PJ Lucro Real).</summary>
    PessoasJuridicasDireitoPrivadoCumulativoLucroReal = 53,

    /// <summary>54 — Recolhimento por Sociedade Cooperativa — Regime Cumulativo (PJ Lucro Real).</summary>
    RecolhimentoSociedadeCooperativaCumulativoLucroReal = 54,

    /// <summary>55 — Retenção por Fabricante de Máquinas e Veículos — Regime Cumulativo (PJ Lucro Real).</summary>
    FabricanteMaquinasVeiculosCumulativoLucroReal = 55,

    /// <summary>59 — Outras Retenções — Regime Cumulativo (PJ Lucro Real, regra específica art. 8º Lei 10.637/2002 e art. 10 Lei 10.833/2003).</summary>
    OutrasRetencoesCumulativoLucroReal = 59,

    /// <summary>99 — Outras Retenções.</summary>
    OutrasRetencoes = 99,
}
