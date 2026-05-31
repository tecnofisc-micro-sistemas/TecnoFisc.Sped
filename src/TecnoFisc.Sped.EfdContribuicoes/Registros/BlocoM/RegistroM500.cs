using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M500 — Crédito de Cofins Relativo ao Período.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 329.
/// </summary>
[RegistroSped(Codigo = "M500", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM500 : RegistroSped
{
    public override string Codigo => "M500";

    /// <summary>Código do Tipo de Crédito apurado no período, conforme Tabela 4.3.6.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CodCred { get; set; }

    /// <summary>Indicador de Crédito Oriundo de: 0 – Operações próprias; 1 – Evento de incorporação, cisão ou fusão.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemCreditoApurado IndCredOri { get; set; }

    /// <summary>Valor da Base de Cálculo do Crédito.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    /// <summary>Alíquota da Cofins (em percentual).</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    /// <summary>Quantidade – Base de cálculo Cofins.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    /// <summary>Alíquota da Cofins (em reais).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    /// <summary>Valor total do crédito apurado no período.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCred { get; set; }

    /// <summary>Valor total dos ajustes de acréscimo.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjusAcres { get; set; }

    /// <summary>Valor total dos ajustes de redução.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjusReduc { get; set; }

    /// <summary>Valor total do crédito diferido no período.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredDifer { get; set; }

    /// <summary>Valor Total do Crédito Disponível relativo ao Período (08 + 09 – 10 – 11).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredDisp { get; set; }

    /// <summary>Indicador de opção de utilização do crédito disponível no período.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDescontoCredito IndDescCred { get; set; }

    /// <summary>Valor do Crédito disponível, descontado da contribuição apurada no próprio período.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredDesc { get; set; }

    /// <summary>Saldo de créditos a utilizar em períodos futuros (12 – 14).</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SldCred { get; set; }
}
