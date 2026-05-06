using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D505 — Complemento da Operação (Códigos 21 e 22) – Cofins.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 218.
/// </summary>
[RegistroSped(Codigo = "D505", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD505 : RegistroSped
{
    public override string Codigo => "D505";

    /// <summary>Código de Situação Tributária referente a Cofins (CST), conforme Tabela III do Anexo Único da IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código da Base de Cálculo do Crédito — Tabela 4.3.7. Valores válidos: 03, 13.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public CodigoBaseCalculoCredito? NatBcCred { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 255)]
    public string? CodCta { get; set; }
}
