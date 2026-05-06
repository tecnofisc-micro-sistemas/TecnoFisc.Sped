using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C481 — Resumo Diário de Documentos Emitidos por ECF – PIS/Pasep (Códigos 02 e 2D).
/// Nível hierárquico 5, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 155.
/// </summary>
[RegistroSped(Codigo = "C481", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC481 : RegistroSped
{
    public override string Codigo => "C481";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 60)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 255)]
    public string? CodCta { get; set; }
}
