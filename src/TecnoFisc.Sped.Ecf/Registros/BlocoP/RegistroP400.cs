using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoP;

/// <summary>Registro P400 - base de cálculo da CSLL no lucro presumido.</summary>
[RegistroSped(Codigo = "P400", Nivel = 3, Bloco = "P")]
public sealed partial class RegistroP400 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P400";

    /// <summary>Código da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor na representação textual da tabela dinâmica.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
