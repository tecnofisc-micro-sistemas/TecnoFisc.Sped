using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M990 - encerramento do Bloco M.</summary>
[RegistroSped(Codigo = "M990", Nivel = 1, Bloco = "M")]
public sealed partial class RegistroM990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M990";

    /// <summary>Quantidade total de registros do Bloco M.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}
