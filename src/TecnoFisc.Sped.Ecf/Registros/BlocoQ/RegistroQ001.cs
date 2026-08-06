using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoQ;

/// <summary>Registro Q001 - abertura do Bloco Q.</summary>
[RegistroSped(Codigo = "Q001", Nivel = 1, Bloco = "Q")]
public sealed partial class RegistroQ001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Q001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
