using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D111 — Processo Referenciado. Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 202.
/// </summary>
[RegistroSped(Codigo = "D111", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD111 : RegistroSped
{
    public override string Codigo => "D111";

    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
