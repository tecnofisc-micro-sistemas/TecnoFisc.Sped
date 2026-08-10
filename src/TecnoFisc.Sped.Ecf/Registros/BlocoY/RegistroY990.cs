using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y990 - encerramento do bloco Y.</summary>
[RegistroSped(Codigo = "Y990", Nivel = 1, Bloco = "Y")]
public sealed partial class RegistroY990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y990";

    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
