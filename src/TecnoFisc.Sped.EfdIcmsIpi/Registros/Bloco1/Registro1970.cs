using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1970 - GIAF 3 - Guia de Informacao e Apuracao de Incentivos Fiscais e
/// Financeiros: Importacao (diferimento na entrada e credito presumido na saida subsequente).
/// Nivel hierarquico 2, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 298-299.
/// </summary>
[RegistroSped(Codigo = "1970", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1970 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1970";

    /// <summary>Indicador da sub-apuracao por tipo de beneficio, conforme tabela 4.7.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorSubApuracaoIcms? IndAp { get; set; }

    /// <summary>Importacoes com ICMS diferido.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G301 { get; set; }

    /// <summary>ICMS diferido nas importacoes.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G302 { get; set; }

    /// <summary>Saidas nao incentivadas de PI.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G303 { get; set; }

    /// <summary>Percentual de incentivo nas saidas para fora do Estado.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G304 { get; set; }

    /// <summary>Saidas incentivadas de PI para fora do Estado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G305 { get; set; }

    /// <summary>ICMS das saidas incentivadas de PI para fora do Estado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G306 { get; set; }

    /// <summary>Credito presumido nas saidas para fora do Estado.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G307 { get; set; }

    /// <summary>Deducao de incentivo da Importacao (credito presumido).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G3T { get; set; }

    /// <summary>Saldo devedor do ICMS antes das deducoes do incentivo.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G308 { get; set; }

    /// <summary>Saldo devedor do ICMS apos deducoes do incentivo.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G309 { get; set; }
}
