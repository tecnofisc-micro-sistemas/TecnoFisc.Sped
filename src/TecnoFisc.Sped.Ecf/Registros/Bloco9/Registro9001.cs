using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco9;

/// <summary>Registro 9001 - abertura do Bloco 9.</summary>
[RegistroSped(Codigo = "9001", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
