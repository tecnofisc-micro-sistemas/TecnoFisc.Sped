using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M610 — Detalhamento da Contribuição para a Seguridade Social – Cofins do Período.
/// Nível hierárquico 3, ocorrência 1:N. Filho de M600.
/// Leiaute aplicável aos fatos geradores a partir de 01/01/2019.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 343.
/// </summary>
[RegistroSped(Codigo = "M610", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM610 : RegistroSped
{
    public override string Codigo => "M610";

    /// <summary>Código da contribuição social apurada no período, conforme Tabela 4.3.5.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public CodigoContribuicaoSocialApurada CodCont { get; set; }

    /// <summary>Valor da Receita Bruta.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecBrt { get; set; }

    /// <summary>Valor da Base de Cálculo da Contribuição, antes de ajustes.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCont { get; set; }

    /// <summary>Valor do total dos ajustes de acréscimo da base de cálculo.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjusAcresBcCofins { get; set; }

    /// <summary>Valor do total dos ajustes de redução da base de cálculo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjusReducBcCofins { get; set; }

    /// <summary>Valor da Base de Cálculo da Contribuição, após os ajustes (Campo 04 + Campo 05 – Campo 06).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcContAjus { get; set; }

    /// <summary>Alíquota da Cofins (em percentual).</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    /// <summary>Quantidade – Base de cálculo Cofins.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    /// <summary>Alíquota da Cofins (em reais).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    /// <summary>Valor total da contribuição social apurada.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContApur { get; set; }

    /// <summary>Valor total dos ajustes de acréscimo da contribuição social apurada.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjusAcres { get; set; }

    /// <summary>Valor total dos ajustes de redução da contribuição social apurada.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjusReduc { get; set; }

    /// <summary>Valor da contribuição a diferir no período.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlContDifer { get; set; }

    /// <summary>Valor da contribuição diferida em períodos anteriores.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlContDiferAnt { get; set; }

    /// <summary>Valor Total da Contribuição do Período (11 + 12 – 13 – 14 + 15).</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContPer { get; set; }
}
