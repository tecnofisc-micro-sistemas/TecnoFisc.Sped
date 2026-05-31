using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I100 — Consolidação das Operações do Período.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 286.
/// </summary>
[RegistroSped(Codigo = "I100", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI100 : RegistroSped
{
    public override string Codigo => "I100";

    /// <summary>Valor total do faturamento/receita bruta no período, correspondente ao CST informado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRec { get; set; }

    /// <summary>Código de Situação Tributária referente à receita (PIS e Cofins combinados).</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CstPisCofins { get; set; }

    /// <summary>Valor total das deduções e exclusões de caráter geral.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlTotDedGer { get; set; }

    /// <summary>Valor total das deduções e exclusões de caráter específico.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlTotDedEsp { get; set; }

    /// <summary>Valor da base de cálculo do PIS/Pasep.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    /// <summary>Alíquota do PIS/Pasep (em percentual).</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Decimais = 2)]
    public decimal? AliqPis { get; set; }

    /// <summary>Valor do PIS/Pasep.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da base de cálculo da Cofins.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    /// <summary>Alíquota da Cofins (em percentual).</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Decimais = 2)]
    public decimal? AliqCofins { get; set; }

    /// <summary>Valor da Cofins.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
