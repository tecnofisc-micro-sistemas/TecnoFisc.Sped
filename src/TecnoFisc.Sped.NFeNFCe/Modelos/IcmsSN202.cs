using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSSN202</c> — Tributação pelo Simples Nacional sem permissão de crédito
/// e com cobrança do ICMS por Substituição Tributária (CSOSN 202 ou 203).
/// </summary>
public sealed record IcmsSN202 : Icms
{
    /// <summary><c>CSOSN</c> — código de situação: 202 (sem crédito + ST) ou 203 (isento por faixa de RB + ST).</summary>
    public required Csosn CSOSN { get; init; }

    // --- Grupo ICMS ST ---

    /// <summary><c>modBCST</c> — modalidade de determinação da BC do ICMS ST: 0=Preço tabelado/máximo, 1=Lista Negativa, 2=Lista Positiva, 3=Lista Neutra, 4=MVA(%), 5=Pauta, 6=Valor da Operação.</summary>
    public required int ModBCST { get; init; }

    /// <summary><c>pMVAST</c> — percentual da Margem de Valor Adicionado ICMS ST (opcional).</summary>
    public decimal? PMVAST { get; init; }

    /// <summary><c>pRedBCST</c> — percentual de redução da BC ICMS ST (opcional).</summary>
    public decimal? PRedBCST { get; init; }

    /// <summary><c>vBCST</c> — valor da BC do ICMS ST.</summary>
    public required decimal VBCST { get; init; }

    /// <summary><c>pICMSST</c> — alíquota do ICMS ST.</summary>
    public required decimal PICMSST { get; init; }

    /// <summary><c>vICMSST</c> — valor do ICMS ST.</summary>
    public required decimal VICMSST { get; init; }

    /// <summary><c>vBCFCPST</c> — valor da base de cálculo do FCP retido por ST (opcional).</summary>
    public decimal? VBCFCPST { get; init; }

    /// <summary><c>pFCPST</c> — percentual do FCP retido por substituição tributária (opcional).</summary>
    public decimal? PFCPST { get; init; }

    /// <summary><c>vFCPST</c> — valor do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VFCPST { get; init; }
}
