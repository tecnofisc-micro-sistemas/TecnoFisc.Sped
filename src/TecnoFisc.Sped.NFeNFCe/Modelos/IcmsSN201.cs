using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSSN201</c> — Tributação pelo Simples Nacional com permissão de crédito
/// e com cobrança do ICMS por Substituição Tributária (CSOSN 201).
/// </summary>
public sealed record IcmsSN201 : Icms
{
    /// <summary><c>CSOSN</c> — código de situação da operação no Simples Nacional (sempre "201" nesta variante).</summary>
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

    // --- Grupo crédito SN ---

    /// <summary><c>pCredSN</c> — alíquota aplicável de cálculo do crédito (Simples Nacional).</summary>
    public required decimal PCredSN { get; init; }

    /// <summary><c>vCredICMSSN</c> — valor crédito do ICMS aproveitável nos termos do art. 23 da LC 123.</summary>
    public required decimal VCredICMSSN { get; init; }
}
