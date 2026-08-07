using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoT;

/// <summary>Registro T030 - período de apuração do lucro arbitrado.</summary>
[RegistroSped(Codigo = "T030", Nivel = 2, Bloco = "T")]
public sealed partial class RegistroT030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "T030";

    /// <summary>Data inicial do período de apuração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_INI")]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do período de apuração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_FIN")]
    public DateOnly DtFin { get; set; }

    /// <summary>Código do trimestre, preservado sem validação tributária condicional.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true, Nome = "PER_APUR")]
    public string? PerApur { get; set; }
}
