using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0900 — Composição das Receitas do Período – Receita Bruta e Demais Receitas.
/// Nível hierárquico 2, ocorrência 1 por arquivo. Obrigatório quando a escrituração for
/// transmitida após o prazo regular (após o 10º dia útil do 2º mês subsequente ao período).
/// Conforme Guia Prático v1.35, p. 91-94.
/// </summary>
[RegistroSped(Codigo = "0900", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0900 : RegistroSped
{
    public override string Codigo => "0900";

    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalBlocoA { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? RecNrbBlocoA { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalBlocoC { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? RecNrbBlocoC { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalBlocoD { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? RecNrbBlocoD { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalBlocoF { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? RecNrbBlocoF { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalBlocoI { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? RecNrbBlocoI { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalBloco1 { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? RecNrbBloco1 { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecTotalPeriodo { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? RecTotalNrbPeriodo { get; set; }
}
