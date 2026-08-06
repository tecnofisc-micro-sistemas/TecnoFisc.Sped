using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X410 - informações de homepage e servidor.</summary>
[RegistroSped(Codigo = "X410", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX410 : RegistroSped
{
    public override string Codigo => "X410";

    /// <summary>País conforme tabela dinâmica, preservado como código lexical.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndHomeDisp { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndServDisp { get; set; }
}
