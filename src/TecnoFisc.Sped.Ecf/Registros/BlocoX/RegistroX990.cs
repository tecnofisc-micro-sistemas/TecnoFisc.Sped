using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X990 - encerramento do Bloco X.</summary>
[RegistroSped(Codigo = "X990", Nivel = 1, Bloco = "X")]
public sealed partial class RegistroX990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X990";

    /// <summary>Quantidade total de registros do Bloco X.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
