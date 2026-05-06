using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1700 — Controle dos Valores Retidos na Fonte – Cofins.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Controla saldos de valores retidos
/// na fonte, de períodos anteriores e do período da atual escrituração, consolidados por
/// natureza da retenção (IND_NAT_RET) e período de recebimento/retenção (PR_REC_RET).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 406.
/// </summary>
[RegistroSped(Codigo = "1700", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1700 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1700";

    /// <summary>Indicador da natureza da retenção na fonte.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorNaturezaRetencao IndNatRet { get; set; }

    /// <summary>Período do recebimento e da retenção no formato MMAAAA.</summary>
    [CampoSped(Ordem = 3, Tamanho = 6, Obrigatorio = true)]
    public string? PrRecRet { get; set; }

    /// <summary>Valor total da retenção efetivamente sofrida referente à natureza e período informados.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetApu { get; set; }

    /// <summary>Valor da retenção deduzida da contribuição devida no período da escrituração e em períodos anteriores (acumulado).</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetDed { get; set; }

    /// <summary>Valor da retenção utilizada mediante pedido de restituição (acumulado).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetPer { get; set; }

    /// <summary>Valor da retenção utilizada mediante declaração de compensação (acumulado).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetDcomp { get; set; }

    /// <summary>Saldo de retenção a utilizar em períodos de apuração futuros (04 – 05 – 06 – 07).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SldRet { get; set; }
}
