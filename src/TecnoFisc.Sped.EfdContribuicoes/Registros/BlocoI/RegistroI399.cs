using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I399 — Processo Referenciado (filho de I300).
/// Nível hierárquico 6, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 293.
/// </summary>
[RegistroSped(Codigo = "I399", Nivel = 6, Bloco = "I")]
public sealed partial class RegistroI399 : RegistroSped
{
    public override string Codigo => "I399";

    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
