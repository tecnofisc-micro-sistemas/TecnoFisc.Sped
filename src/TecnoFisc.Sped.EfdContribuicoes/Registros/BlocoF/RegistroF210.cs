using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F210 — Operações da Atividade Imobiliária - Custo Orçado da Unidade Imobiliária Vendida.
/// Nível hierárquico 4, ocorrência 1:N (filho de F200). Preenchido apenas quando IND_OPER do F200
/// for 03 ou 04 (venda de unidade em construção).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 255.
/// </summary>
[RegistroSped(Codigo = "F210", Nivel = 4, Bloco = "F")]
public sealed partial class RegistroF210 : RegistroSped
{
    public override string Codigo => "F210";

    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCusOrc { get; set; }

    /// <summary>Parcela do custo orçado sem direito ao crédito (mão-de-obra física e aquisições não sujeitas a contribuição).</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlExc { get; set; }

    /// <summary>Base de Cálculo do Crédito sobre o Custo Orçado Ajustado (Campo 02 – 03).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCusOrcAju { get; set; }

    /// <summary>Base de Cálculo do Crédito sobre o Custo Orçado no mês, proporcionalizada pela receita recebida.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCred { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    /// <summary>Crédito sobre o custo orçado a ser utilizado no período da escrituração – PIS/PASEP (Campo 05 x 07).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredPisUtil { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 9, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    /// <summary>Crédito sobre o custo orçado a ser utilizado no período da escrituração – COFINS (Campo 05 x 10).</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredCofinsUtil { get; set; }
}
