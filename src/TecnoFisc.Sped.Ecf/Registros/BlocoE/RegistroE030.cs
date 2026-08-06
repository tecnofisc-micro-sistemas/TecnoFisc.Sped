using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E030 - identificação do período calculado com base nas ECD.</summary>
[RegistroSped(Codigo = "E030", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E030";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? PerApur { get; set; }
}
