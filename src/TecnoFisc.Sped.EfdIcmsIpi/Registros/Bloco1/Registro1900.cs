using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1900 - Indicador de sub-apuracao do ICMS.
/// Nivel hierarquico 2, ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 289.
/// </summary>
[RegistroSped(Codigo = "1900", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1900";

    /// <summary>Indicador de outra apuracao do ICMS.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSubApuracaoIcms? IndApurIcms { get; set; }

    /// <summary>Descricao complementar da outra apuracao do ICMS.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescrComplOutApur { get; set; }
}
