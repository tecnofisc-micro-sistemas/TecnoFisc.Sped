using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1960 - GIAF 1 - Guia de Informacao e Apuracao de Incentivos Fiscais e
/// Financeiros: Industria (credito presumido).
/// Nivel hierarquico 2, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 296-298.
/// </summary>
[RegistroSped(Codigo = "1960", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1960 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1960";

    /// <summary>Indicador da sub-apuracao por tipo de beneficio, conforme tabela 4.7.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorSubApuracaoIcms? IndAp { get; set; }

    /// <summary>Percentual de credito presumido.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G101 { get; set; }

    /// <summary>Saidas nao incentivadas de PI.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G102 { get; set; }

    /// <summary>Saidas incentivadas de PI.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G103 { get; set; }

    /// <summary>Saidas incentivadas de PI para fora do Nordeste.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G104 { get; set; }

    /// <summary>Saldo devedor do ICMS antes das deducoes do incentivo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G105 { get; set; }

    /// <summary>Saldo devedor do ICMS relativo a faixa incentivada de PI.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G106 { get; set; }

    /// <summary>Credito presumido nas saidas incentivadas de PI para fora do Nordeste.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G107 { get; set; }

    /// <summary>
    /// Saldo devedor relativo a faixa incentivada de PI apos o credito presumido nas saidas
    /// para fora do Nordeste.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G108 { get; set; }

    /// <summary>Credito presumido.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G109 { get; set; }

    /// <summary>Deducao de incentivo da Industria (credito presumido).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G110 { get; set; }

    /// <summary>Saldo devedor do ICMS apos deducoes.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G111 { get; set; }
}
