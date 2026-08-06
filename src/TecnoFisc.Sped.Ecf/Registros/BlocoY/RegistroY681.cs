using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y681 - informações dinâmicas dos optantes pelo Refis.</summary>
[RegistroSped(Codigo = "Y681", Nivel = 3, Bloco = "Y")]
public sealed partial class RegistroY681 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y681";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
