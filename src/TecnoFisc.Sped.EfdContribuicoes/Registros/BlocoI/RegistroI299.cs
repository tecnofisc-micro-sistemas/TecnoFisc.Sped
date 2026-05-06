using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I299 — Processo Referenciado (filho de I200).
/// Nível hierárquico 5, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 290.
/// </summary>
[RegistroSped(Codigo = "I299", Nivel = 5, Bloco = "I")]
public sealed partial class RegistroI299 : RegistroSped
{
    public override string Codigo => "I299";

    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
