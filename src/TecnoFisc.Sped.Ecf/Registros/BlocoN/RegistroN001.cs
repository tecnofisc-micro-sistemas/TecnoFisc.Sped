using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N001 - abertura do bloco N.</summary>
[RegistroSped(Codigo = "N001", Nivel = 1, Bloco = "N")]
public sealed partial class RegistroN001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
