using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D105 — Complemento do Documento de Transporte (Códigos 07, 08, 8B, 09, 10, 11, 26, 27,
/// 57, 63 e 67) – Cofins. Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 200.
/// </summary>
[RegistroSped(Codigo = "D105", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD105 : RegistroSped
{
    public override string Codigo => "D105";

    /// <summary>Indicador da natureza do frete contratado — um registro por indicador.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorNaturezaFrete IndNatFrt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código de Situação Tributária da Cofins (CST), conforme Tabela III do Anexo Único da IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    /// <summary>Código da Base de Cálculo do Crédito — Tabela 4.3.7. Informar quando CST representar direito a crédito.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2)]
    public CodigoBaseCalculoCredito? NatBcCred { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 255)]
    public string? CodCta { get; set; }
}
