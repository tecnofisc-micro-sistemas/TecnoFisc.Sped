using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A120 — Informação Complementar - Operações de Importação. Nível hierárquico 4,
/// ocorrência 1:N. Conforme Guia Prático v1.35, p. 100.
/// </summary>
[RegistroSped(Codigo = "A120", Nivel = 4, Bloco = "A")]
public sealed partial class RegistroA120 : RegistroSped
{
    public override string Codigo => "A120";

    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotServ { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcPis { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisImp { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtPagPis { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCofins { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsImp { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtPagCofins { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorLocalExecucaoServico LocExeServ { get; set; }
}
