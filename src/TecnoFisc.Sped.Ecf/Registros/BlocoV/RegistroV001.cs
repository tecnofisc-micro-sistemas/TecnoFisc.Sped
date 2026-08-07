using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoV;

/// <summary>Registro V001 - abertura do Bloco V.</summary>
[RegistroSped(Codigo = "V001", Nivel = 1, Bloco = "V")]
public sealed partial class RegistroV001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "V001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_DAD")]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
