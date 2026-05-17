using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1710 - Documentos fiscais cancelados/inutilizados.
/// Nivel hierarquico 3, ocorrencia um ou varios por Registro 1700. Conforme Guia Pratico
/// EFD-ICMS/IPI V3.0.6, pp. 287-288.
/// </summary>
[RegistroSped(Codigo = "1710", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1710 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1710";

    /// <summary>Numero do dispositivo autorizado inutilizado inicial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]
    public long NumDocIni { get; set; }

    /// <summary>Numero do dispositivo autorizado inutilizado final.</summary>
    [CampoSped(Ordem = 3, Tamanho = 12, Obrigatorio = true)]
    public long NumDocFin { get; set; }
}
