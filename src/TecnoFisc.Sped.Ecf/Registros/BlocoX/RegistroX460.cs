using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X460 - inovação e desenvolvimento tecnológico.</summary>
[RegistroSped(Codigo = "X460", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX460 : RegistroSped
{
    public override string Codigo => "X460";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4, Nome = "VALOR")]
    public string? Valor { get; set; }
}
