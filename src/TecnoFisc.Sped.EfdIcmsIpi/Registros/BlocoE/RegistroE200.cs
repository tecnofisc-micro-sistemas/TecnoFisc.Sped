using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E200 — Período de Apuração do ICMS — Substituição Tributária.
/// Nível hierárquico 2, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 214.
/// </summary>
[RegistroSped(Codigo = "E200", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E200";

    /// <summary>Sigla da unidade da federação a que se refere a apuração do ICMS ST.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Data inicial a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
