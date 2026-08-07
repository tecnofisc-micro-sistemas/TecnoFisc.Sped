using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoT;

/// <summary>Registro T120 - base de cálculo do IRPJ no lucro arbitrado.</summary>
[RegistroSped(Codigo = "T120", Nivel = 3, Bloco = "T")]
public sealed partial class RegistroT120 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "T120";

    /// <summary>Código da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Valor na representação textual da tabela dinâmica.</summary>
    [CampoSped(Ordem = 4, Nome = "VALOR")]
    public string? Valor { get; set; }
}
