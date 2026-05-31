using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C505 — Complemento da Operação (Códigos 06, 28 e 29) – Cofins.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 169.
/// </summary>
[RegistroSped(Codigo = "C505", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC505 : RegistroSped
{
    public override string Codigo => "C505";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código da Base de Cálculo do Crédito — Tabela 4.3.7. Valores válidos: 01, 02, 04, 13.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public CodigoBaseCalculoCredito? NatBcCred { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCofins { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofins { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 255)]
    public string? CodCta { get; set; }
}
