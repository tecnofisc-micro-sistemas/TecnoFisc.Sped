using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco9;

/// <summary>Registro 9999 - encerramento do arquivo digital.</summary>
[RegistroSped(Codigo = "9999", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9999 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9999";

    /// <summary>Quantidade total de registros do arquivo.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}
