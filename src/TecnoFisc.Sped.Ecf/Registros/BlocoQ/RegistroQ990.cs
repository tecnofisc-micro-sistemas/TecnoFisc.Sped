using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoQ;

/// <summary>Registro Q990 - encerramento do Bloco Q.</summary>
[RegistroSped(Codigo = "Q990", Nivel = 1, Bloco = "Q")]
public sealed partial class RegistroQ990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Q990";

    /// <summary>Quantidade total de registros do Bloco Q.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
