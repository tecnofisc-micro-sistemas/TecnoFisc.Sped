using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D150 — Complemento do Conhecimento Aéreo de Cargas (Código 10).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 172.
/// </summary>
[RegistroSped(Codigo = "D150", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D150";

    /// <summary>Código do município de origem do serviço, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 2, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>Código do município de destino, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Identificação da aeronave (DAC).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? VeicId { get; set; }

    /// <summary>Número do voo.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public int? Viagem { get; set; }

    /// <summary>Indicador do tipo de tarifa aplicada.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1)]
    public IndicadorTarifaAerea? IndTfa { get; set; }

    /// <summary>Peso taxado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlPesoTx { get; set; }

    /// <summary>Valor da taxa terrestre.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlTxTerr { get; set; }

    /// <summary>Valor da taxa de redespacho.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlTxRed { get; set; }

    /// <summary>Outros valores.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlOut { get; set; }

    /// <summary>Valor da taxa "ad valorem".</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlTxAdv { get; set; }
}
