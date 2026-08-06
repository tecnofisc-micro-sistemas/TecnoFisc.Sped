using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M365 - processos referentes ao lançamento da Parte A do e-Lacs.</summary>
[RegistroSped(Codigo = "M365", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM365 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M365";

    /// <summary>Tipo do processo judicial ou administrativo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public TipoProcessoEcf IndProc { get; set; }

    /// <summary>Número do processo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }
}
