using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1910 - Periodo da sub-apuracao do ICMS.
/// Nivel hierarquico 3, ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 290.
/// </summary>
[RegistroSped(Codigo = "1910", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1910 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1910";

    /// <summary>Data inicial da sub-apuracao (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final da sub-apuracao (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
