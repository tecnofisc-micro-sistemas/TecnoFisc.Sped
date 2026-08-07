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
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true, Nome = "PAIS")]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true, Nome = "IND_HOME_DISP")]
    public IndicadorSimNao IndHomeDisp { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "IND_SERV_DISP")]
    public IndicadorSimNao IndServDisp { get; set; }
}
