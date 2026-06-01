using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSSN900</c> — Outros (CSOSN 900). Aplica-se a CRT = 1 (Simples Nacional)
/// ou CRT = 4 (MEI). Todos os grupos de cálculo são opcionais no XSD (<c>minOccurs="0"</c>),
/// incluindo <c>orig</c>. Como <c>Icms.Orig</c> é obrigatório no modelo, quando o elemento
/// está ausente o parser aplica o default <c>OrigemMercadoria.Nacional</c> (0).
/// </summary>
public sealed record IcmsSN900 : Icms
{
    /// <summary><c>CSOSN</c> — código de situação da operação no Simples Nacional (sempre "900" nesta variante).</summary>
    public required Csosn CSOSN { get; init; }

    // --- Grupo ICMS próprio (opcional) ---

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS: 0=MVA(%), 1=Pauta, 2=Preço Tabelado Máximo, 3=Valor da Operação (opcional).</summary>
    public int? ModBC { get; init; }

    /// <summary><c>vBC</c> — valor da BC do ICMS (opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pRedBC</c> — percentual de redução da BC (opcional).</summary>
    public decimal? PRedBC { get; init; }

    /// <summary><c>pICMS</c> — alíquota do ICMS (opcional).</summary>
    public decimal? PICMS { get; init; }

    /// <summary><c>vICMS</c> — valor do ICMS (opcional).</summary>
    public decimal? VICMS { get; init; }

    // --- Grupo ICMS ST (opcional) ---

    /// <summary><c>modBCST</c> — modalidade de determinação da BC do ICMS ST (opcional): 0=Preço tabelado/máximo, 1=Lista Negativa, 2=Lista Positiva, 3=Lista Neutra, 4=MVA(%), 5=Pauta, 6=Valor da Operação.</summary>
    public int? ModBCST { get; init; }

    /// <summary><c>pMVAST</c> — percentual da Margem de Valor Adicionado ICMS ST (opcional).</summary>
    public decimal? PMVAST { get; init; }

    /// <summary><c>pRedBCST</c> — percentual de redução da BC ICMS ST (opcional).</summary>
    public decimal? PRedBCST { get; init; }

    /// <summary><c>vBCST</c> — valor da BC do ICMS ST (opcional).</summary>
    public decimal? VBCST { get; init; }

    /// <summary><c>pICMSST</c> — alíquota do ICMS ST (opcional).</summary>
    public decimal? PICMSST { get; init; }

    /// <summary><c>vICMSST</c> — valor do ICMS ST (opcional).</summary>
    public decimal? VICMSST { get; init; }

    /// <summary><c>vBCFCPST</c> — valor da base de cálculo do FCP retido por ST (opcional).</summary>
    public decimal? VBCFCPST { get; init; }

    /// <summary><c>pFCPST</c> — percentual do FCP retido por substituição tributária (opcional).</summary>
    public decimal? PFCPST { get; init; }

    /// <summary><c>vFCPST</c> — valor do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VFCPST { get; init; }

    // --- Grupo crédito SN (opcional) ---

    /// <summary><c>pCredSN</c> — alíquota aplicável de cálculo do crédito (Simples Nacional) (opcional).</summary>
    public decimal? PCredSN { get; init; }

    /// <summary><c>vCredICMSSN</c> — valor crédito do ICMS aproveitável nos termos do art. 23 da LC 123 (opcional).</summary>
    public decimal? VCredICMSSN { get; init; }
}
