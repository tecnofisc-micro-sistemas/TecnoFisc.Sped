using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoL;

/// <summary>Registro L210 - linha da composição de custos.</summary>
[RegistroSped(Codigo = "L210", Nivel = 3, Bloco = "L")]
public sealed partial class RegistroL210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "L210";

    /// <summary>Código da conta de custos da tabela dinâmica.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da conta de custos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Valor textual da linha dinâmica, inclusive rótulos e cálculos representados como texto.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? Valor { get; set; }
}
