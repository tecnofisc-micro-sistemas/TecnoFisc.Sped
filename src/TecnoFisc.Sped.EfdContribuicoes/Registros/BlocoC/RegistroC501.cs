using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C501 — Complemento da Operação (Códigos 06, 28 e 29) – PIS/Pasep.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 167.
/// </summary>
[RegistroSped(Codigo = "C501", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC501 : RegistroSped
{
    public override string Codigo => "C501";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código da Base de Cálculo do Crédito — Tabela 4.3.7. Valores válidos: 01, 02, 04, 13.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public CodigoBaseCalculoCredito? NatBcCred { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcPis { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqPis { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 255)]
    public string? CodCta { get; set; }
}
