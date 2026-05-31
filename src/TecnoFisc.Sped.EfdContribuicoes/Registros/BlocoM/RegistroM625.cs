using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M625 — Detalhamento dos Ajustes da Cofins Apurada.
/// Nível hierárquico 5, ocorrência 1:N (filho de M620).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 356.
/// </summary>
[RegistroSped(Codigo = "M625", Nivel = 5, Bloco = "M")]
public sealed partial class RegistroM625 : RegistroSped
{
    public override string Codigo => "M625";

    /// <summary>Detalhamento do valor do ajuste da contribuição (campo VL_AJ do M620).</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DetValorAj { get; set; }

    /// <summary>Código de Situação Tributária – Cofins referente à operação detalhada neste registro.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2)]
    public int? CstCofins { get; set; }

    /// <summary>Detalhamento da base de cálculo geradora de ajuste de contribuição.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3)]
    public decimal? DetBcCred { get; set; }

    /// <summary>Detalhamento da alíquota a que se refere o ajuste de contribuição.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4)]
    public decimal? DetAliq { get; set; }

    /// <summary>Data da operação a que se refere o ajuste informado neste registro (ddmmaaaa).</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtOperAj { get; set; }

    /// <summary>Descrição da(s) operação(ões) a que se refere o valor de DET_VALOR_AJ.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? DescAj { get; set; }

    /// <summary>Código da conta contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 8, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Informação complementar.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
