using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C001 - abertura do bloco C.</summary>
[RegistroSped(Codigo = "C001", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
