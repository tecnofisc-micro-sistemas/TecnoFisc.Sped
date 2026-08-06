using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X490 - Polo Industrial de Manaus e Amazônia Ocidental.</summary>
[RegistroSped(Codigo = "X490", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX490 : RegistroSped
{
    public override string Codigo => "X490";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
