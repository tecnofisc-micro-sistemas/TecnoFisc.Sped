using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoL;

/// <summary>Registro L001 - abertura do bloco L.</summary>
[RegistroSped(Codigo = "L001", Nivel = 1, Bloco = "L")]
public sealed partial class RegistroL001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "L001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
