using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C820 — Detalhamento do Cupom Fiscal Eletrônico (Código 59) – PIS/Pasep e Cofins
/// Apurado por Unidade de Medida de Produto.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 182.
/// </summary>
[RegistroSped(Codigo = "C820", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC820 : RegistroSped
{
    public override string Codigo => "C820";

    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 255)]
    public string? CodCta { get; set; }
}
