using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1500 — Controle de Créditos Fiscais – Cofins.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Controla saldos de créditos
/// fiscais de períodos anteriores à escrituração atual, segregados por COD_CRED e período.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 396.
/// </summary>
[RegistroSped(Codigo = "1500", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1500";

    /// <summary>Período de apuração do crédito no formato MMAAAA.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? PerApuCred { get; set; }

    /// <summary>Indicador da origem do crédito (operações próprias ou transferido por sucessão).</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorOrigemCreditoFiscal OrigCred { get; set; }

    /// <summary>CNPJ da pessoa jurídica cedente do crédito (preenchido somente quando ORIG_CRED = 02).</summary>
    [CampoSped(Ordem = 4, Tamanho = 14)]
    public Cnpj? CnpjSuc { get; set; }

    /// <summary>Código do tipo de crédito, conforme Tabela 4.3.6.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public CodigoTipoCredito CodCred { get; set; }

    /// <summary>Valor total do crédito apurado na escrituração fiscal digital (Registro M500) ou em demonstrativo DACON de período anterior.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredApu { get; set; }

    /// <summary>Valor do crédito extemporâneo apurado (Registro 1501), referente ao período anterior informado no campo PER_APU_CRED.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredExtApu { get; set; }

    /// <summary>Valor total do crédito apurado (VL_CRED_APU + VL_CRED_EXT_APU).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotCredApu { get; set; }

    /// <summary>Valor do crédito utilizado mediante desconto em período(s) anterior(es).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredDescPaAnt { get; set; }

    /// <summary>Valor do crédito utilizado mediante pedido de ressarcimento em período(s) anterior(es).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredPerPaAnt { get; set; }

    /// <summary>Valor do crédito utilizado mediante declaração de compensação intermediária (crédito de exportação) em período(s) anterior(es).</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredDcompPaAnt { get; set; }

    /// <summary>Saldo do crédito disponível para utilização neste período de escrituração (08 – 09 – 10 – 11).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SdCredDispEfd { get; set; }

    /// <summary>Valor do crédito descontado neste período de escrituração.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredDescEfd { get; set; }

    /// <summary>Valor do crédito objeto de pedido de ressarcimento (PER) neste período de escrituração.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredPerEfd { get; set; }

    /// <summary>Valor do crédito utilizado mediante declaração de compensação intermediária neste período de escrituração.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredDcompEfd { get; set; }

    /// <summary>Valor do crédito transferido em evento de cisão, fusão ou incorporação.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredTrans { get; set; }

    /// <summary>Valor do crédito utilizado por outras formas.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredOut { get; set; }

    /// <summary>Saldo de créditos a utilizar em período de apuração futuro (12 – 13 – 14 – 15 – 16 – 17).</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SldCredFim { get; set; }
}
