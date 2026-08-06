using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M415 - processos referentes ao lançamento da Parte B.</summary>
[RegistroSped(Codigo = "M415", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM415 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M415";

    /// <summary>Tipo do processo judicial ou administrativo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public TipoProcessoEcf IndProc { get; set; }

    /// <summary>Número do processo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }
}
