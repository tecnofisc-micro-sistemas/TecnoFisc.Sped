using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U990 - encerramento do Bloco U.</summary>
[RegistroSped(Codigo = "U990", Nivel = 1, Bloco = "U")]
public sealed partial class RegistroU990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U990";

    /// <summary>Quantidade total de registros do Bloco U.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
