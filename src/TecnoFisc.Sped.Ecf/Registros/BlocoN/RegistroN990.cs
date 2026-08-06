using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N990 - encerramento do Bloco N.</summary>
[RegistroSped(Codigo = "N990", Nivel = 1, Bloco = "N")]
public sealed partial class RegistroN990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N990";

    /// <summary>Quantidade total de registros do Bloco N.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}
