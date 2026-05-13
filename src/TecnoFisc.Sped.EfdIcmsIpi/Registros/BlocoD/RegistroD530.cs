using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D530 — Terminal Faturado.
/// Nível hierárquico 3, ocorrência 1:N (por registro D500).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 202-203.
/// </summary>
[RegistroSped(Codigo = "D530", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD530 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D530";

    /// <summary>Indicador do tipo de serviço prestado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoServico? IndServ { get; set; }

    /// <summary>Data em que se iniciou a prestação do serviço no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIniServ { get; set; }

    /// <summary>Data em que se encerrou a prestação do serviço no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinServ { get; set; }

    /// <summary>Período fiscal da prestação do serviço no formato MMAAAA.</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? PerFiscal { get; set; }

    /// <summary>Código de área do terminal faturado.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? CodArea { get; set; }

    /// <summary>Identificação do terminal faturado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public long? Terminal { get; set; }
}
