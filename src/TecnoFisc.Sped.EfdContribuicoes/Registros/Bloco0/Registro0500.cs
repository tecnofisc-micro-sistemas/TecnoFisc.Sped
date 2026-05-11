using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0500 — Plano de Contas Contábeis. Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático v1.35, p. 89-91.
/// </summary>
[RegistroSped(Codigo = "0500", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0500 : RegistroSped
{
    public override string Codigo => "0500";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoNaturezaContaContabil CodNatCc { get; set; }

    /// <summary>Indicador do tipo de conta: S - Sintética (grupo de contas); A - Analítica (conta).</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public string? IndCta { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int Nivel { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? NomeCta { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0)]
    public string? CodCtaRef { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 14)]
    public Cnpj? CnpjEst { get; set; }
}
