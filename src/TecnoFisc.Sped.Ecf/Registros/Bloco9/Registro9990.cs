using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco9;

/// <summary>Registro 9990 - encerramento do Bloco 9.</summary>
[RegistroSped(Codigo = "9990", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9990";

    /// <summary>Quantidade total de registros do Bloco 9.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
