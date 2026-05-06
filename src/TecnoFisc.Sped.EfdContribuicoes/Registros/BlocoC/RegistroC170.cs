using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C170 — Complemento do Documento - Itens do Documento (Códigos 01, 1B, 04 e 55).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 117.
/// </summary>
[RegistroSped(Codigo = "C170", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC170 : RegistroSped
{
    public override string Codigo => "C170";

    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumItem { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescrCompl { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 5)]
    public decimal? Qtd { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 6)]
    public string? Unid { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 1)]
    public IndicadorMovimentacaoFisica? IndMov { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 3)]
    public int? CstIcms { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 10)]
    public string? CodNat { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 6, Decimais = 2)]
    public decimal? AliqSt { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 1)]
    public IndicadorApuracaoIpi? IndApur { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 2)]
    public string? CstIpi { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 3)]
    public string? CodEnq { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIpi { get; set; }

    [CampoSped(Ordem = 23, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIpi { get; set; }

    [CampoSped(Ordem = 24, Tamanho = 0, Decimais = 2)]
    public decimal? VlIpi { get; set; }

    [CampoSped(Ordem = 25, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 26, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 27, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 28, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    [CampoSped(Ordem = 29, Tamanho = 0, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    [CampoSped(Ordem = 30, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 31, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 32, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 33, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 34, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    [CampoSped(Ordem = 35, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    [CampoSped(Ordem = 36, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 37, Tamanho = 255)]
    public string? CodCta { get; set; }
}
