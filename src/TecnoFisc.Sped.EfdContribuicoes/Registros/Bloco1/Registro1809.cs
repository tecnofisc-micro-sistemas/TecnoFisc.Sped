using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1809 — Processo Referenciado.
/// Nível hierárquico 3, ocorrência 1:N. Filho do Registro 1800. Registra o processo administrativo
/// ou judicial que autoriza adoção de tratamento tributário (CST, base de cálculo ou alíquota)
/// diverso do previsto na legislação para a incorporação imobiliária no regime RET.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 409.
/// </summary>
[RegistroSped(Codigo = "1809", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1809 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1809";

    /// <summary>Número do processo judicial ou do processo administrativo (até 20 caracteres).</summary>
    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
