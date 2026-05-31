using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1920 - Sub-apuracao do ICMS.
/// Nivel hierarquico 4, ocorrencia um por periodo. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 290.
/// </summary>
[RegistroSped(Codigo = "1920", Nivel = 4, Bloco = "1")]
public sealed partial class Registro1920 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1920";

    /// <summary>Valor total dos debitos por saidas e prestacoes com debito do imposto.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotTransfDebitosOa { get; set; }

    /// <summary>Valor total de ajustes a debito.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotAjDebitosOa { get; set; }

    /// <summary>Valor total de ajustes de estornos de creditos.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlEstornosCredOa { get; set; }

    /// <summary>Valor total dos creditos por entradas e aquisicoes com credito do imposto.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotTransfCreditosOa { get; set; }

    /// <summary>Valor total de ajustes a credito.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotAjCreditosOa { get; set; }

    /// <summary>Valor total de ajustes de estornos de debitos.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlEstornosDebOa { get; set; }

    /// <summary>Valor total de saldo credor do periodo anterior.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredorAntOa { get; set; }

    /// <summary>Valor do saldo devedor apurado.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldApuradoOa { get; set; }

    /// <summary>Valor total de deducoes.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotDed { get; set; }

    /// <summary>Valor total de ICMS a recolher.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsRecolherOa { get; set; }

    /// <summary>Valor total de saldo credor a transportar para o periodo seguinte.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredorTranspOa { get; set; }

    /// <summary>Valores recolhidos ou a recolher, extra-apuracao.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DebEspOa { get; set; }
}
