using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X365 - contrapartes nas transações controladas.</summary>
[RegistroSped(Codigo = "X365", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX365 : RegistroSped
{
    public override string Codigo => "X365";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? Identificador { get; set; }

    [CampoSped(Ordem = 3)]
    public string? NomeEnt { get; set; }
}
