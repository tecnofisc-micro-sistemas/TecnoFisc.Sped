using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K990 - encerramento do bloco K.</summary>
[RegistroSped(Codigo = "K990", Nivel = 1, Bloco = "K")]
public sealed partial class RegistroK990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K990";

    /// <summary>Quantidade total de registros do bloco K.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
