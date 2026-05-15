using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1980 - GIAF 4 - Guia de Informacao e Apuracao de Incentivos Fiscais e
/// Financeiros: Central de Distribuicao (entradas/saidas).
/// Nivel hierarquico 2, ocorrencia 1. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 300-301.
/// </summary>
[RegistroSped(Codigo = "1980", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1980 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1980";

    /// <summary>Indicador da sub-apuracao por tipo de beneficio, conforme tabela 4.7.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorSubApuracaoIcms? IndAp { get; set; }

    /// <summary>Entradas: percentual de incentivo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G401 { get; set; }

    /// <summary>Entradas nao incentivadas de PI.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G402 { get; set; }

    /// <summary>Entradas incentivadas de PI.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G403 { get; set; }

    /// <summary>Saidas: percentual de incentivo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G404 { get; set; }

    /// <summary>Saidas nao incentivadas de PI.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G405 { get; set; }

    /// <summary>Saidas incentivadas de PI.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G406 { get; set; }

    /// <summary>Saldo devedor do ICMS antes das deducoes do incentivo.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G407 { get; set; }

    /// <summary>Credito presumido nas entradas incentivadas de PI.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G408 { get; set; }

    /// <summary>Credito presumido nas saidas incentivadas de PI.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G409 { get; set; }

    /// <summary>Deducao de incentivo da Central de Distribuicao.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G410 { get; set; }

    /// <summary>Saldo devedor do ICMS apos deducoes do incentivo.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G411 { get; set; }

    /// <summary>Indice de recolhimento da central de distribuicao.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G412 { get; set; }
}
