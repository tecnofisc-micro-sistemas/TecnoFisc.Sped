namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código de Tipo de Crédito — Tabela 4.3.6 do Guia Prático EFD Contribuições.
/// Codifica os tipos de crédito apurado no período (Bloco M) ou de controle de
/// créditos de períodos anteriores (Bloco 1). Campo COD_CRED.
/// </summary>
/// <remarks>
/// Grupos de códigos:
/// <list type="bullet">
/// <item>1xx — Vinculados à receita tributada no mercado interno.</item>
/// <item>2xx — Vinculados à receita não tributada no mercado interno.</item>
/// <item>3xx — Vinculados à receita de exportação.</item>
/// </list>
/// O sufixo 99 (199, 299, 399) é "Outros" do grupo respectivo.
/// </remarks>
public enum CodigoTipoCredito
{
    /// <summary>101 — Mercado interno tributado – Alíquota Básica.</summary>
    MercadoInternoTributadoAliquotaBasica = 101,

    /// <summary>102 — Mercado interno tributado – Alíquotas Diferenciadas.</summary>
    MercadoInternoTributadoAliquotasDiferenciadas = 102,

    /// <summary>103 — Mercado interno tributado – Alíquota por Unidade de Produto.</summary>
    MercadoInternoTributadoAliquotaUnidade = 103,

    /// <summary>104 — Mercado interno tributado – Estoque de Abertura.</summary>
    MercadoInternoTributadoEstoqueAbertura = 104,

    /// <summary>105 — Mercado interno tributado – Aquisição de Embalagens para revenda.</summary>
    MercadoInternoTributadoAquisicaoEmbalagensRevenda = 105,

    /// <summary>106 — Mercado interno tributado – Presumido da Agroindústria.</summary>
    MercadoInternoTributadoPresumidoAgroindustria = 106,

    /// <summary>107 — Mercado interno tributado – Outros Créditos Presumidos.</summary>
    MercadoInternoTributadoOutrosPresumidos = 107,

    /// <summary>108 — Mercado interno tributado – Importação.</summary>
    MercadoInternoTributadoImportacao = 108,

    /// <summary>109 — Mercado interno tributado – Atividade Imobiliária.</summary>
    MercadoInternoTributadoAtividadeImobiliaria = 109,

    /// <summary>199 — Mercado interno tributado – Outros.</summary>
    MercadoInternoTributadoOutros = 199,

    /// <summary>201 — Mercado interno não tributado – Alíquota Básica.</summary>
    MercadoInternoNaoTributadoAliquotaBasica = 201,

    /// <summary>202 — Mercado interno não tributado – Alíquotas Diferenciadas.</summary>
    MercadoInternoNaoTributadoAliquotasDiferenciadas = 202,

    /// <summary>203 — Mercado interno não tributado – Alíquota por Unidade de Produto.</summary>
    MercadoInternoNaoTributadoAliquotaUnidade = 203,

    /// <summary>204 — Mercado interno não tributado – Estoque de Abertura.</summary>
    MercadoInternoNaoTributadoEstoqueAbertura = 204,

    /// <summary>205 — Mercado interno não tributado – Aquisição de Embalagens para revenda.</summary>
    MercadoInternoNaoTributadoAquisicaoEmbalagensRevenda = 205,

    /// <summary>206 — Mercado interno não tributado – Presumido da Agroindústria.</summary>
    MercadoInternoNaoTributadoPresumidoAgroindustria = 206,

    /// <summary>207 — Mercado interno não tributado – Outros Créditos Presumidos.</summary>
    MercadoInternoNaoTributadoOutrosPresumidos = 207,

    /// <summary>208 — Mercado interno não tributado – Importação.</summary>
    MercadoInternoNaoTributadoImportacao = 208,

    /// <summary>299 — Mercado interno não tributado – Outros.</summary>
    MercadoInternoNaoTributadoOutros = 299,

    /// <summary>301 — Exportação – Alíquota Básica.</summary>
    ExportacaoAliquotaBasica = 301,

    /// <summary>302 — Exportação – Alíquotas Diferenciadas.</summary>
    ExportacaoAliquotasDiferenciadas = 302,

    /// <summary>303 — Exportação – Alíquota por Unidade de Produto.</summary>
    ExportacaoAliquotaUnidade = 303,

    /// <summary>304 — Exportação – Estoque de Abertura.</summary>
    ExportacaoEstoqueAbertura = 304,

    /// <summary>305 — Exportação – Aquisição de Embalagens para revenda.</summary>
    ExportacaoAquisicaoEmbalagensRevenda = 305,

    /// <summary>306 — Exportação – Presumido da Agroindústria.</summary>
    ExportacaoPresumidoAgroindustria = 306,

    /// <summary>307 — Exportação – Outros Créditos Presumidos.</summary>
    ExportacaoOutrosPresumidos = 307,

    /// <summary>308 — Exportação – Importação.</summary>
    ExportacaoImportacao = 308,

    /// <summary>399 — Exportação – Outros.</summary>
    ExportacaoOutros = 399,
}
