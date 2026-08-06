using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X292 - linhas da tabela dinâmica de operações com o exterior.</summary>
[RegistroSped(Codigo = "X292", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX292 : RegistroSped
{
    public override string Codigo => "X292";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
