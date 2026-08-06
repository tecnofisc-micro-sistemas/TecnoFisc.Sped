using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U001 - abertura do Bloco U.</summary>
[RegistroSped(Codigo = "U001", Nivel = 1, Bloco = "U")]
public sealed partial class RegistroU001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
