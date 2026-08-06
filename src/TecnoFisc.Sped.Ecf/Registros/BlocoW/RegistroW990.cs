using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W990 - encerramento do Bloco W.</summary>
[RegistroSped(Codigo = "W990", Nivel = 1, Bloco = "W")]
public sealed partial class RegistroW990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W990";

    /// <summary>Quantidade total de registros do Bloco W.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}
