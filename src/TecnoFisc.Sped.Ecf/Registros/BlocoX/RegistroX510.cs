using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X510 - Areas de Livre Comercio (ALC).</summary>
[RegistroSped(Codigo = "X510", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X510";

    /// <summary>Codigo conforme a tabela dinamica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descricao conforme a tabela dinamica do Sped.</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor textual da linha dinamica.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
