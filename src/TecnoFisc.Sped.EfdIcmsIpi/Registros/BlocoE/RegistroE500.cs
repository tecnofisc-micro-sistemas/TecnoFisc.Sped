using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E500 — Período de Apuração do IPI.
/// Nível hierárquico 2, ocorrência 1:N por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 231.
/// </summary>
[RegistroSped(Codigo = "E500", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E500";

    /// <summary>Indicador de período de apuração do IPI: 0=Mensal, 1=Decendial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorApuracaoIpi? IndApur { get; set; }

    /// <summary>Data inicial a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
