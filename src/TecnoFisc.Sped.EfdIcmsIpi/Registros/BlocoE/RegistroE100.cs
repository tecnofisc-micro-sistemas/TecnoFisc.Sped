using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E100 — Período de Apuração do ICMS.
/// Nível hierárquico 2, ocorrência 1:N por bloco E. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 206.
/// </summary>
[RegistroSped(Codigo = "E100", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E100";

    /// <summary>Data inicial a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
