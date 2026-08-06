using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoP;

/// <summary>Registro P030 - período de apuração do lucro presumido.</summary>
[RegistroSped(Codigo = "P030", Nivel = 2, Bloco = "P")]
public sealed partial class RegistroP030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P030";

    /// <summary>Data inicial do período de apuração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do período de apuração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Código do período de apuração, preservado sem validação tributária condicional.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? PerApur { get; set; }
}
