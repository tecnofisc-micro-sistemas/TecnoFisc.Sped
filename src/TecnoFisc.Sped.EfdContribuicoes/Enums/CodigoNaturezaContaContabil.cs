namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código da natureza da conta/grupo de contas — campo COD_NAT_CC do Registro 0500.
/// Valores válidos pelo Guia Prático v1.35, p. 89: <c>01, 02, 03, 04, 05, 09</c>.
/// A largura SPED é fixa em dois caracteres (Tam=002*), de modo que o serializador
/// zero-pad o underlying int.
/// </summary>
public enum CodigoNaturezaContaContabil
{
    /// <summary>01 — Contas de ativo.</summary>
    ContasDeAtivo = 1,

    /// <summary>02 — Contas de passivo.</summary>
    ContasDePassivo = 2,

    /// <summary>03 — Patrimônio líquido.</summary>
    PatrimonioLiquido = 3,

    /// <summary>04 — Contas de resultado.</summary>
    ContasDeResultado = 4,

    /// <summary>05 — Contas de compensação.</summary>
    ContasDeCompensacao = 5,

    /// <summary>09 — Outras.</summary>
    Outras = 9,
}
