using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M810 — Detalhamento das Receitas Isentas, Não Alcançadas pela Incidência da Contribuição,
/// Sujeitas a Alíquota Zero ou de Vendas com Suspensão – Cofins.
/// Nível hierárquico 3, ocorrência 1:N (filho de M800).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 361.
/// </summary>
[RegistroSped(Codigo = "M810", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM810 : RegistroSped
{
    public override string Codigo => "M810";

    /// <summary>Natureza da Receita, conforme tabelas 4.3.10 a 4.3.16 por CST orientador.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? NatRec { get; set; }

    /// <summary>Valor da receita bruta no período, relativo à natureza da receita (NAT_REC).</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRec { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 4, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Descrição Complementar da Natureza da Receita.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? DescCompl { get; set; }
}
