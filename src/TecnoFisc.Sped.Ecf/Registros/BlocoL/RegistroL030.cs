using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoL;

/// <summary>Registro L030 - período e forma de apuração.</summary>
[RegistroSped(Codigo = "L030", Nivel = 2, Bloco = "L")]
public sealed partial class RegistroL030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "L030";

    /// <summary>Data inicial do período de apuração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_INI")]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do período de apuração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_FIN")]
    public DateOnly DtFin { get; set; }

    /// <summary>Código do período de apuração.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true, Nome = "PER_APUR")]
    public string? PerApur { get; set; }
}
