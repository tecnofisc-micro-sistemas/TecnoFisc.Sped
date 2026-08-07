using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y001 - abertura do Bloco Y.</summary>
[RegistroSped(Codigo = "Y001", Nivel = 1, Bloco = "Y")]
public sealed partial class RegistroY001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y001";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_DAD")]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
