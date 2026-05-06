using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D501 — Complemento da Operação (Códigos 21 e 22) – PIS/Pasep.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 216.
/// </summary>
[RegistroSped(Codigo = "D501", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD501 : RegistroSped
{
    public override string Codigo => "D501";

    /// <summary>Código de Situação Tributária do PIS/Pasep (CST), conforme Tabela II do Anexo Único da IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código da Base de Cálculo do Crédito — Tabela 4.3.7. Valores válidos: 03, 13.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public CodigoBaseCalculoCredito? NatBcCred { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 255)]
    public string? CodCta { get; set; }
}
