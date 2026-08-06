using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoJ;

/// <summary>Registro J053 - subcontas correlatas.</summary>
[RegistroSped(Codigo = "J053", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ053 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J053";

    /// <summary>Código do grupo formado pela conta e suas subcontas.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodIdt { get; set; }

    /// <summary>Código da subconta correlata.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCntCorr { get; set; }

    /// <summary>Código da natureza da subconta correlata.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? NatSubCnt { get; set; }
}
