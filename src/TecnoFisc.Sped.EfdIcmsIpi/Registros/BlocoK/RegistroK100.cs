using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K100 — Período de Apuração do ICMS/IPI.
/// Nível hierárquico 2, ocorrência vários por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 250.
/// </summary>
[RegistroSped(Codigo = "K100", Nivel = 2, Bloco = "K")]
public sealed partial class RegistroK100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K100";

    /// <summary>Data inicial a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
