using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoT;

/// <summary>Registro T001 - abertura do Bloco T.</summary>
[RegistroSped(Codigo = "T001", Nivel = 1, Bloco = "T")]
public sealed partial class RegistroT001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "T001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_DAD")]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
