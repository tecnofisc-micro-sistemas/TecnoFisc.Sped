using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K001 - abertura do bloco K.</summary>
[RegistroSped(Codigo = "K001", Nivel = 1, Bloco = "K")]
public sealed partial class RegistroK001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
