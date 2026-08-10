using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoL;

/// <summary>Registro L200 - método de avaliação do estoque final.</summary>
[RegistroSped(Codigo = "L200", Nivel = 3, Bloco = "L")]
public sealed partial class RegistroL200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "L200";

    /// <summary>Método de avaliação do estoque final.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_AVAL_ESTOQ")]
    public MetodoAvaliacaoEstoque IndAvalEstoq { get; set; }
}
