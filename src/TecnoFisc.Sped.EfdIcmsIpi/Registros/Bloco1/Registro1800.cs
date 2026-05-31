using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1800 - DCTA - Demonstrativo de Credito do ICMS sobre transporte aereo.
/// Nivel hierarquico 2, ocorrencia unica por arquivo. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 288.
/// </summary>
[RegistroSped(Codigo = "1800", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1800 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1800";

    /// <summary>Valor das prestacoes de cargas tributadas.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCarga { get; set; }

    /// <summary>Valor das prestacoes de passageiros/cargas nao tributadas.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlPass { get; set; }

    /// <summary>Valor total do faturamento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlFat { get; set; }

    /// <summary>Indice para rateio.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 6, Obrigatorio = true)]
    public decimal IndRat { get; set; }

    /// <summary>Valor total dos creditos do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsAnt { get; set; }

    /// <summary>Valor da base de calculo do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcms { get; set; }

    /// <summary>Valor do ICMS apurado no calculo.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsApur { get; set; }

    /// <summary>Valor da base de calculo do ICMS apurada.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcmsApur { get; set; }

    /// <summary>Valor da diferenca a ser levada a estorno de credito na apuracao.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDif { get; set; }
}
