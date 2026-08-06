using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X375 - informações relacionadas aos métodos.</summary>
[RegistroSped(Codigo = "X375", Nivel = 3, Bloco = "X", IntroduzidoEm = (int)LayoutEcf.V010)]
public sealed partial class RegistroX375 : RegistroSped
{
    public override string Codigo => "X375";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor de tabela dinâmica, preservado sem coerção.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
