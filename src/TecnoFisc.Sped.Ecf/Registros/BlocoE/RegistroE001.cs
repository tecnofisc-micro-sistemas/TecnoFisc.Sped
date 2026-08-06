using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E001 - abertura do bloco E.</summary>
[RegistroSped(Codigo = "E001", Nivel = 1, Bloco = "E")]
public sealed partial class RegistroE001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
