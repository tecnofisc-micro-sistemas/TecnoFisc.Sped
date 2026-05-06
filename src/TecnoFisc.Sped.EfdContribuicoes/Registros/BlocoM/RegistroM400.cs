using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M400 — Receitas Isentas, não Alcançadas pela Incidência da Contribuição,
/// Sujeitas a Alíquota Zero ou de Vendas com Suspensão – PIS/Pasep.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 326.
/// </summary>
[RegistroSped(Codigo = "M400", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM400 : RegistroSped
{
    public override string Codigo => "M400";

    /// <summary>Código de Situação Tributária – CST das receitas sem incidência ou alíquota zero (04-09).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    /// <summary>Valor total da receita bruta no período.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotRec { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 4, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Descrição Complementar da Natureza da Receita.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? DescCompl { get; set; }
}
