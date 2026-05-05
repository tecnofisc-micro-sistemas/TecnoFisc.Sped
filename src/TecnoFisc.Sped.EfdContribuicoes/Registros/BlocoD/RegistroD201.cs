using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D201 — Totalização do Resumo Diário – PIS/Pasep. Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 204.
/// </summary>
[RegistroSped(Codigo = "D201", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD201 : RegistroSped
{
    public override string Codigo => "D201";

    /// <summary>Código de Situação Tributária do PIS/Pasep (CST), conforme Tabela II da IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 255)]
    public string? CodCta { get; set; }
}
