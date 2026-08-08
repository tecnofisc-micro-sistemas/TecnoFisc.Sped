using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M315 - processo judicial ou administrativo do lançamento.</summary>
[RegistroSped(Codigo = "M315", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM315 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M315";

    /// <summary>Tipo do Processo: 1 - Judicial 2 – Administrativo</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_PROC")]
    public TipoProcessoEcf IndProc { get; set; }

    /// <summary>Número do Processo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true, Nome = "NUM_PROC")]
    public string? NumProc { get; set; }
}
