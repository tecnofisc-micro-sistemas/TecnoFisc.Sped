using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C155 — Detalhe dos Saldos Periódicos Recuperados. Nível hierárquico 4, ocorrência 1:N.
/// Demonstra os saldos recuperados da ECD anterior de acordo com o período informado no Registro C150.
/// Regras de validação cruzada (responsabilidade do consumidor, ARCHITECTURE §2.3):
/// <list type="bullet">
/// <item><c>REGRA_CONTA_C155_INEXISTENTE_I155</c> — verifica existência da conta/centro de custos no Registro I155.</item>
/// <item><c>REGRA_NATUREZA_CONTA_C155</c> — verifica que a natureza da conta (<c>C050.COD_NAT</c>) é igual à informada no Registro I155.</item>
/// </list>
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 97.
/// </summary>
[RegistroSped(Codigo = "C155", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC155 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C155";

    /// <summary>Código da conta analítica.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaRec { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcusRec { get; set; }

    /// <summary>Valor do saldo inicial do período.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldIniRec { get; set; }

    /// <summary>Indicador da situação do saldo inicial: D = Devedor, C = Credor.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorDebitoCredito? IndDcIniRec { get; set; }

    /// <summary>Valor total dos débitos no período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlDebRec { get; set; }

    /// <summary>Valor total dos créditos no período.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredRec { get; set; }

    /// <summary>Valor do saldo final do período.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldFinRec { get; set; }

    /// <summary>Indicador da situação do saldo final do período: D = Devedor, C = Credor.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1)]
    public IndicadorDebitoCredito? IndDcFinRec { get; set; }
}
