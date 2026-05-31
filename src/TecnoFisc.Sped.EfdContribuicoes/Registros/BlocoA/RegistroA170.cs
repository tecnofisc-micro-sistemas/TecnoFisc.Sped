using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A170 — Complemento do Documento - Itens do Documento. Nível hierárquico 4,
/// ocorrência 1:N. Conforme Guia Prático v1.35, p. 101.
/// </summary>
[RegistroSped(Codigo = "A170", Nivel = 4, Bloco = "A")]
public sealed partial class RegistroA170 : RegistroSped
{
    public override string Codigo => "A170";

    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public int NumItem { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescrCompl { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 2)]
    public string? NatBcCred { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 1)]
    public IndicadorOrigemCredito? IndOrigCred { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 6, Decimais = 2)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 255)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 255)]
    public string? CodCcus { get; set; }
}
