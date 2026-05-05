using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C489 — Processo Referenciado. Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 159.
/// </summary>
[RegistroSped(Codigo = "C489", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC489 : RegistroSped
{
    public override string Codigo => "C489";

    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
