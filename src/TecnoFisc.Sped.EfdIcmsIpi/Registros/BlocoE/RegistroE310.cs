using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E310 — Apuração do Fundo de Combate à Pobreza e do ICMS Diferencial de Alíquota — UF Origem/Destino EC 87/15.
/// Nível hierárquico 3, ocorrência um por período. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 223-227.
/// </summary>
[RegistroSped(Codigo = "E310", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E310";

    /// <summary>Indicador de movimento — 0 sem operações, 1 com operações.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoFcpDifal IndMovFcpDifal { get; set; }

    /// <summary>Valor do saldo credor de período anterior do ICMS Diferencial de Alíquota da UF de Origem/Destino.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredAntDifal { get; set; }

    /// <summary>Valor total dos débitos por saídas e prestações com débito do ICMS Difal da UF de Origem/Destino.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotDebitosDifal { get; set; }

    /// <summary>Valor total dos ajustes "Outros débitos" e "Estorno de créditos" do ICMS Difal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDebDifal { get; set; }

    /// <summary>Valor total dos créditos do ICMS Difal devido à UF de Origem/Destino.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotCreditosDifal { get; set; }

    /// <summary>Valor total dos ajustes "Outros créditos" e "Estorno de débitos" do ICMS Difal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutCredDifal { get; set; }

    /// <summary>Valor total de saldo devedor do ICMS Difal antes das deduções.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldDevAntDifal { get; set; }

    /// <summary>Valor total das deduções do ICMS Diferencial de Alíquota da UF de Origem/Destino.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDeducoesDifal { get; set; }

    /// <summary>Valor recolhido ou a recolher referente ao ICMS Difal da UF de Origem/Destino.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecolDifal { get; set; }

    /// <summary>Saldo credor do ICMS Difal a transportar para o período seguinte.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredTransportarDifal { get; set; }

    /// <summary>Valores recolhidos ou a recolher, extra-apuração, do ICMS Difal.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DebEspDifal { get; set; }

    /// <summary>Valor do saldo credor de período anterior do FCP.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredAntFcp { get; set; }

    /// <summary>Valor total dos débitos FCP por saídas e prestações.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotDebFcp { get; set; }

    /// <summary>Valor total dos ajustes "Outros débitos FCP" e "Estorno de créditos FCP".</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDebFcp { get; set; }

    /// <summary>Valor total dos créditos FCP por entradas.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotCredFcp { get; set; }

    /// <summary>Valor total dos ajustes "Outros créditos FCP" e "Estorno de débitos FCP".</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutCredFcp { get; set; }

    /// <summary>Valor total de saldo devedor FCP antes das deduções.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldDevAntFcp { get; set; }

    /// <summary>Valor total das deduções FCP.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDeducoesFcp { get; set; }

    /// <summary>Valor recolhido ou a recolher referente ao FCP.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecolFcp { get; set; }

    /// <summary>Saldo credor FCP a transportar para o período seguinte.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredTransportarFcp { get; set; }

    /// <summary>Valores recolhidos ou a recolher, extra-apuração, do FCP.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DebEspFcp { get; set; }
}
