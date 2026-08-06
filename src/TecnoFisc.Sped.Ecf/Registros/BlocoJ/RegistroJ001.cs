using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoJ;

/// <summary>Registro J001 - abertura do bloco J.</summary>
[RegistroSped(Codigo = "J001", Nivel = 1, Bloco = "J")]
public sealed partial class RegistroJ001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
