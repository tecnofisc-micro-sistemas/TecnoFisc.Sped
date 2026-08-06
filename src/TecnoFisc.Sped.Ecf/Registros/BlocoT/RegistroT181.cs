using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoT;

/// <summary>Registro T181 - cálculo da CSLL no lucro arbitrado.</summary>
[RegistroSped(Codigo = "T181", Nivel = 3, Bloco = "T")]
public sealed partial class RegistroT181 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "T181";

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
