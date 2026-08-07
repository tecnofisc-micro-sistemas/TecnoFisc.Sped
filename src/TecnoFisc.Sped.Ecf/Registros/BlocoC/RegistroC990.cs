using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C990 - encerramento do bloco C.</summary>
[RegistroSped(Codigo = "C990", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C990";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
