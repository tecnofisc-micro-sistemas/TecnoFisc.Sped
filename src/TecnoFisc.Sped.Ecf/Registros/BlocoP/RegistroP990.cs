using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoP;

/// <summary>Registro P990 - encerramento do Bloco P.</summary>
[RegistroSped(Codigo = "P990", Nivel = 1, Bloco = "P")]
public sealed partial class RegistroP990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P990";

    /// <summary>Quantidade total de registros do Bloco P.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
