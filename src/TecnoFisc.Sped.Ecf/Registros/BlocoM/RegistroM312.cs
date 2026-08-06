using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M312 - número do lançamento contábil relacionado.</summary>
[RegistroSped(Codigo = "M312", Nivel = 5, Bloco = "M")]
public sealed partial class RegistroM312 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M312";

    /// <summary>Número do Lançamento Descrito na ECD (Escrituração Contábil Digital) no campo 2 (NUM_LCTO)registro “I200 – Lançamento Contábil”.</summary>
    [CampoSped(Ordem = 2, Tamanho = 50, Obrigatorio = true)]
    public string? NumLcto { get; set; }
}
