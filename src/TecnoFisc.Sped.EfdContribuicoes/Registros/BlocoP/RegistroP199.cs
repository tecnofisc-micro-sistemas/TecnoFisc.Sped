using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P199 — Processo Referenciado. Nível hierárquico 4, ocorrência 1:N (filho de P100).
/// Informação de processo judicial ou administrativo que autoriza tratamento tributário,
/// base de cálculo ou alíquota diversa da prevista na legislação. Conforme Guia Prático v1.35, p. 370.
/// </summary>
[RegistroSped(Codigo = "P199", Nivel = 4, Bloco = "P")]
public sealed partial class RegistroP199 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P199";

    /// <summary>Número do processo judicial ou ato concessório (até 20 caracteres).</summary>
    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo: 1 Justiça Federal, 3 Receita Federal, 9 Outros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
