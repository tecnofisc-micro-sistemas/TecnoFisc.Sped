using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D605 — Complemento da Consolidação da Prestação de Serviços (Códigos 21 e 22) – Cofins.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 226.
/// </summary>
[RegistroSped(Codigo = "D605", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD605 : RegistroSped
{
    public override string Codigo => "D605";

    /// <summary>Código de classificação do item do serviço de comunicação ou telecomunicação, conforme Tabela 4.4.1 do Ato COTEPE/ICMS nº 09/2008.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? CodClass { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Código de Situação Tributária referente a Cofins (CST), conforme Tabela III do Anexo Único da IN RFB nº 1.009/2010. Valores válidos: 01, 02, 06, 07, 08, 09, 49, 99.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 255)]
    public string? CodCta { get; set; }
}
