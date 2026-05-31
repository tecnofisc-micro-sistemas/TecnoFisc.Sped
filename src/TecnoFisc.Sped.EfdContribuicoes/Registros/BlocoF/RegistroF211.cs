using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F211 — Processo Referenciado (filho de F200). Nível hierárquico 4, ocorrência 1:N.
/// Informa processo judicial ou administrativo que autoriza tratamento tributário distinto.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 257.
/// </summary>
[RegistroSped(Codigo = "F211", Nivel = 4, Bloco = "F")]
public sealed partial class RegistroF211 : RegistroSped
{
    public override string Codigo => "F211";

    /// <summary>Identificação do processo ou ato concessório.</summary>
    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
