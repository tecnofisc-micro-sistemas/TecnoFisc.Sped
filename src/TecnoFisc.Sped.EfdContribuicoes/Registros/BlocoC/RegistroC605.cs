using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C605 — Complemento da Consolidação Diária (Códigos 06, 28 e 29) – Documentos de Saídas – Cofins.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 175.
/// </summary>
[RegistroSped(Codigo = "C605", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC605 : RegistroSped
{
    public override string Codigo => "C605";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCofins { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCofins { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 255)]
    public string? CodCta { get; set; }
}
