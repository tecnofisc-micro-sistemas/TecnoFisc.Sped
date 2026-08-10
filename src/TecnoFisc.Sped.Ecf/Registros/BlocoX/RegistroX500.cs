using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X500 - Zonas de Processamento de Exportacao (ZPE).</summary>
[RegistroSped(Codigo = "X500", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X500";

    /// <summary>Codigo conforme a tabela dinamica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descricao conforme a tabela dinamica do Sped.</summary>
    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Valor textual da linha dinamica.</summary>
    [CampoSped(Ordem = 4, Nome = "VALOR")]
    public string? Valor { get; set; }
}
