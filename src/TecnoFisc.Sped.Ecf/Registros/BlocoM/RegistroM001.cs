using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M001 - abertura do bloco M.</summary>
[RegistroSped(Codigo = "M001", Nivel = 1, Bloco = "M")]
public sealed partial class RegistroM001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
