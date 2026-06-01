using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C485 — Resumo Diário de Documentos Emitidos por ECF – Cofins (Códigos 02 e 2D).
/// Nível hierárquico 5, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 157.
/// </summary>
[RegistroSped(Codigo = "C485", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC485 : RegistroSped
{
    public override string Codigo => "C485";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 60)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 255)]
    public string? CodCta { get; set; }
}
