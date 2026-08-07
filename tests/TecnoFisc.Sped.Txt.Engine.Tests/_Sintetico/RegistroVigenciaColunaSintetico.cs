using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

/// <summary>
/// Campo 3 só existe a partir da versão 12; campos 2 e 4 existem sempre. Um arquivo que
/// declara versão anterior mas traz a coluna 3 preenchida não pode deslocar a coluna 4.
/// </summary>
[RegistroSped(Codigo = "A300", Nivel = 2, Bloco = "A")]
public sealed partial class RegistroVigenciaColunaSintetico : RegistroSped
{
    public override string Codigo => "A300";

    [CampoSped(Ordem = 2, Nome = "ANTES")]
    public string? Antes { get; set; }

    [CampoSped(Ordem = 3, Nome = "NOVO", DesdeVersao = 12)]
    public string? Novo { get; set; }

    [CampoSped(Ordem = 4, Nome = "DEPOIS")]
    public string? Depois { get; set; }
}
