using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C491 — Detalhamento da Consolidação de Documentos Emitidos por ECF (Códigos 02, 2D, 59 e 60) – PIS/Pasep.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 160.
/// </summary>
[RegistroSped(Codigo = "C491", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC491 : RegistroSped
{
    public override string Codigo => "C491";

    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 255)]
    public string? CodCta { get; set; }
}
