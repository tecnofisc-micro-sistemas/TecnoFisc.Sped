using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C320 — Registro Analítico do Resumo Diário das NF de Venda a Consumidor (código 02).
/// Consolida os valores diários das notas fiscais de venda ao consumidor, não emitidas por ECF,
/// agrupados por combinação de CST_ICMS, CFOP e alíquota do ICMS.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 108.
/// </summary>
[RegistroSped(Codigo = "C320", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC320 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C320";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1. Primeiro caractere sempre será zero.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação. Deve iniciar por "5" (saídas internas).</summary>
    [CampoSped(Ordem = 3, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor total acumulado das operações na combinação de CST_ICMS, CFOP e alíquota do ICMS, incluídas despesas acessórias e acréscimos.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlOpr { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS, referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS, referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor não tributado em função da redução da base de cálculo do ICMS, referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlRedBc { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 9, Tamanho = 6)]
    public string? CodObs { get; set; }
}
