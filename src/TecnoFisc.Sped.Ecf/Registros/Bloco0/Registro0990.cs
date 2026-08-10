using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0990 — encerramento do bloco 0.</summary>
[RegistroSped(Codigo = "0990", Nivel = 1, Bloco = "0")]
public sealed partial class Registro0990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0990";

    /// <summary>Quantidade total de linhas do bloco 0.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "QTD_LIN")]
    public int QtdLin { get; set; }
}
