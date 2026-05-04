using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0205 — Alteração do Item. Nível hierárquico 4, ocorrência 1:N.
/// Filho do Registro 0200. Conforme Guia Prático v1.35, p. 85-86.
/// </summary>
[RegistroSped(Codigo = "0205", Nivel = 4, Bloco = "0")]
public sealed partial class Registro0205 : RegistroSped
{
    public override string Codigo => "0205";

    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? DescrAntItem { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFim { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodAntItem { get; set; }
}
