using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1700 - Documentos fiscais utilizados.
/// Nivel hierarquico 2, ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, pp. 286-287.
/// </summary>
[RegistroSped(Codigo = "1700", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1700 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1700";

    /// <summary>Codigo do dispositivo autorizado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public CodigoDispositivoAutorizado? CodDisp { get; set; }

    /// <summary>Codigo do modelo do dispositivo autorizado, conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Serie do dispositivo autorizado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subserie do dispositivo autorizado.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public string? Sub { get; set; }

    /// <summary>Numero do dispositivo autorizado utilizado inicial.</summary>
    [CampoSped(Ordem = 6, Tamanho = 12, Obrigatorio = true)]
    public long NumDocIni { get; set; }

    /// <summary>Numero do dispositivo autorizado utilizado final.</summary>
    [CampoSped(Ordem = 7, Tamanho = 12, Obrigatorio = true)]
    public long NumDocFin { get; set; }

    /// <summary>Numero da autorizacao, conforme dispositivo autorizado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 60, Obrigatorio = true)]
    public string? NumAut { get; set; }
}
