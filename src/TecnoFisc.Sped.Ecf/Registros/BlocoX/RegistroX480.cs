using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X480 - benefícios fiscais, parte I.</summary>
[RegistroSped(Codigo = "X480", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX480 : RegistroSped
{
    public override string Codigo => "X480";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2)]
    public decimal? Valor { get; set; }
}
