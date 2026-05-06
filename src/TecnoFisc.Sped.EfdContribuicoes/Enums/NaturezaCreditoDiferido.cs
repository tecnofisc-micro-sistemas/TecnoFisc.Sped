namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Natureza do Crédito Diferido vinculado à receita tributada no mercado interno —
/// campo NAT_CRED_DESC do Registro M300 do Guia Prático EFD Contribuições v1.35, p. 324.
/// </summary>
public enum NaturezaCreditoDiferido
{
    /// <summary>01 — Crédito a Alíquota Básica.</summary>
    CreditoAliquotaBasica = 1,

    /// <summary>02 — Crédito a Alíquota Diferenciada.</summary>
    CreditoAliquotaDiferenciada = 2,

    /// <summary>03 — Crédito a Alíquota por Unidade de Produto.</summary>
    CreditoAliquotaUnidadeProduto = 3,

    /// <summary>04 — Crédito Presumido da Agroindústria.</summary>
    CreditoPresumidoAgroindustria = 4,
}
