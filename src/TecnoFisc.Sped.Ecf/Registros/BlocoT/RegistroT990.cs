using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoT;

/// <summary>Registro T990 - encerramento do Bloco T.</summary>
[RegistroSped(Codigo = "T990", Nivel = 1, Bloco = "T")]
public sealed partial class RegistroT990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "T990";

    /// <summary>Quantidade total de registros do Bloco T.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
