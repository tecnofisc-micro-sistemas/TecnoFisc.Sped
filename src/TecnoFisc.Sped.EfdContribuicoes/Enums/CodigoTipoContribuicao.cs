namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código indicador do tipo de contribuição apurada no período — campo COD_TIPO_CONT do Registro 0110.
/// Valores válidos pelo Guia Prático v1.35, p. 72: <c>1, 2</c>.
/// </summary>
public enum CodigoTipoContribuicao
{
    /// <summary>1 — Apuração da Contribuição Exclusivamente a Alíquota Básica (0,65%/1,65% PIS e 3%/7,6% Cofins).</summary>
    AliquotaBasica = 1,

    /// <summary>2 — Apuração da Contribuição a Alíquotas Específicas (diferenciadas e/ou por Unidade de Medida de Produto).</summary>
    AliquotasEspecificas = 2,
}
