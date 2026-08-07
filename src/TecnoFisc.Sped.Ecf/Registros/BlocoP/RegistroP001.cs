using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoP;

/// <summary>Registro P001 - abertura do Bloco P.</summary>
[RegistroSped(Codigo = "P001", Nivel = 1, Bloco = "P")]
public sealed partial class RegistroP001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_DAD")]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
