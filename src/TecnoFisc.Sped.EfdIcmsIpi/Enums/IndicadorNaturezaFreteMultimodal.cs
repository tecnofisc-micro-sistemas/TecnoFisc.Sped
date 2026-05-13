namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador da natureza do frete — campo IND_NAT_FRT do Registro D170.
/// Classifica se o Conhecimento Multimodal de Cargas (cód. 26) pode ser negociado
/// em instituição financeira (desconto). Guia Prático EFD-ICMS/IPI V3.0.6, p. 176.
/// </summary>
public enum IndicadorNaturezaFreteMultimodal
{
    /// <summary>0 — Negociável.</summary>
    Negociavel = 0,

    /// <summary>1 — Não negociável.</summary>
    NaoNegociavel = 1,
}
