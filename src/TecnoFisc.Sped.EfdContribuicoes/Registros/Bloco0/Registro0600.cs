using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0600 — Centro de Custos. Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático v1.35, p. 91.
/// </summary>
[RegistroSped(Codigo = "0600", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0600 : RegistroSped
{
    public override string Codigo => "0600";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? Ccus { get; set; }
}
