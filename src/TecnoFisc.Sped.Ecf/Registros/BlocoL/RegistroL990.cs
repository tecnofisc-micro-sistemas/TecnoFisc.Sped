using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoL;

/// <summary>Registro L990 - encerramento do bloco L.</summary>
[RegistroSped(Codigo = "L990", Nivel = 1, Bloco = "L")]
public sealed partial class RegistroL990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "L990";

    /// <summary>Quantidade total de registros do bloco L.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
