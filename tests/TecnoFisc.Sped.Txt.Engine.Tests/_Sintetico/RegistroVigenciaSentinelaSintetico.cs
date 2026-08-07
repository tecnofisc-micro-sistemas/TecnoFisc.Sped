using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

/// <summary>
/// Registro introduzido na versão 12 do leiaute (<c>IntroduzidoEm = 12</c>) — usado para provar
/// que um arquivo declarando versão anterior no <c>0000</c> produz uma sentinela
/// <see cref="RegistroNaoReconhecido"/> em vez de descartar a linha em silêncio (achado 2 do
/// PR 531). <see cref="RegistroVigenciaSentinelaFilhoSintetico"/> (A410) é filho de nível maior,
/// para provar que o corte de subárvore também vira sentinela.
/// </summary>
[RegistroSped(Codigo = "A400", Nivel = 2, Bloco = "A", IntroduzidoEm = 12)]
public sealed class RegistroVigenciaSentinelaSintetico : RegistroSped
{
    public override string Codigo => "A400";

    [CampoSped(Ordem = 2)]
    public string? Descricao { get; set; }
}

[RegistroSped(Codigo = "A410", Nivel = 3, Bloco = "A")]
public sealed class RegistroVigenciaSentinelaFilhoSintetico : RegistroSped
{
    public override string Codigo => "A410";

    [CampoSped(Ordem = 2)]
    public string? Descricao { get; set; }
}
