using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D205 — Totalização do Resumo Diário – Cofins. Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 206.
/// </summary>
[RegistroSped(Codigo = "D205", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD205 : RegistroSped
{
    public override string Codigo => "D205";

    /// <summary>Código de Situação Tributária da Cofins (CST), conforme Tabela III da IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 255)]
    public string? CodCta { get; set; }
}
