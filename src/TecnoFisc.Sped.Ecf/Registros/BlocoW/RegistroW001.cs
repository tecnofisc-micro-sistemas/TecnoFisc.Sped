using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W001 - abertura do Bloco W.</summary>
[RegistroSped(Codigo = "W001", Nivel = 1, Bloco = "W")]
public sealed partial class RegistroW001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W001";

    /// <summary>Indicador de movimento do bloco.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_DAD")]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
