using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X001 - abertura do bloco X.</summary>
[RegistroSped(Codigo = "X001", Nivel = 1, Bloco = "X")]
public sealed partial class RegistroX001 : RegistroSped
{
    public override string Codigo => "X001";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
