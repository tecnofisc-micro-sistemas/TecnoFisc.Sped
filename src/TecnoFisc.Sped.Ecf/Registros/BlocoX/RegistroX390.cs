using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X390 - origem e aplicação de recursos das imunes e isentas.</summary>
[RegistroSped(Codigo = "X390", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX390 : RegistroSped
{
    public override string Codigo => "X390";

    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Valor de tabela dinâmica, preservado sem coerção.</summary>
    [CampoSped(Ordem = 4, Nome = "VALOR")]
    public string? Valor { get; set; }
}
