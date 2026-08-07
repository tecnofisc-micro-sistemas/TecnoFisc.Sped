using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoJ;

/// <summary>Registro J990 - encerramento do bloco J.</summary>
[RegistroSped(Codigo = "J990", Nivel = 1, Bloco = "J")]
public sealed partial class RegistroJ990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J990";

    /// <summary>Quantidade total de registros do bloco J.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
