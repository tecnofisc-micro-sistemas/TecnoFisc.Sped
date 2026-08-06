using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoV;

/// <summary>Registro V990 - encerramento do Bloco V.</summary>
[RegistroSped(Codigo = "V990", Nivel = 1, Bloco = "V")]
public sealed partial class RegistroV990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "V990";

    /// <summary>Quantidade total de registros do Bloco V.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinM { get; set; }
}
