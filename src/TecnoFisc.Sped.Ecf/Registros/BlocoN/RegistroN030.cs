using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N030 - período e forma de apuração do IRPJ e da CSLL.</summary>
[RegistroSped(Codigo = "N030", Nivel = 2, Bloco = "N")]
public sealed partial class RegistroN030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N030";

    /// <summary>Data inicial do período de apuração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do período de apuração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Código do período de apuração declarado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? PerApur { get; set; }
}
