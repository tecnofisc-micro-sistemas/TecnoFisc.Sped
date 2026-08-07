using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E990 - encerramento do bloco E.</summary>
[RegistroSped(Codigo = "E990", Nivel = 1, Bloco = "E")]
public sealed partial class RegistroE990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E990";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
