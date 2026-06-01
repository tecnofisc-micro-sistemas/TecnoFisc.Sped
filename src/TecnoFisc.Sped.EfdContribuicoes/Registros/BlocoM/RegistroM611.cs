using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M611 — Sociedades Cooperativas – Composição da Base de Cálculo – Cofins.
/// Nível hierárquico 4, ocorrência 1:1. Filho de M610.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 352.
/// </summary>
[RegistroSped(Codigo = "M611", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM611 : RegistroSped
{
    public override string Codigo => "M611";

    /// <summary>Indicador do tipo de sociedade cooperativa.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorTipoCooperativa IndTipCoop { get; set; }

    /// <summary>Valor da Base de Cálculo da Contribuição, conforme Registros escriturados nos Blocos A, C, D e F, antes das Exclusões das Sociedades Cooperativas.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcContAntExcCoop { get; set; }

    /// <summary>Valor de Exclusão Específica das Cooperativas em Geral, decorrente das Sobras Apuradas na DRE.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlExcCoopGer { get; set; }

    /// <summary>Valor das Exclusões da Base de Cálculo Específica do Tipo da Sociedade Cooperativa, conforme Campo 02.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlExcEspCoop { get; set; }

    /// <summary>Valor da Base de Cálculo, Após as Exclusões Específicas da Sociedade Cooperativa (03 – 04 – 05).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCont { get; set; }
}
