using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M362 - números dos lançamentos relacionados à conta contábil.</summary>
[RegistroSped(Codigo = "M362", Nivel = 5, Bloco = "M")]
public sealed partial class RegistroM362 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M362";

    /// <summary>Número do lançamento descrito na ECD.</summary>
    [CampoSped(Ordem = 2, Tamanho = 50, Obrigatorio = true)]
    public string? NumLcto { get; set; }
}
