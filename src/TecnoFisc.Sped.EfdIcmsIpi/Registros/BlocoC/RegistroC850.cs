using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C850 — Registro Analítico do CF-e-SAT (cód. 59).
/// Agrupa os registros C810 por combinação de CST_ICMS, CFOP e alíquota do ICMS.
/// Nível hierárquico 3, ocorrência 1:N por registro C800.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 156-157.
/// </summary>
[RegistroSped(Codigo = "C850", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC850 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C850";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação do agrupamento de itens.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor total do CF-e na combinação de CST_ICMS, CFOP e alíquota do ICMS, correspondente ao somatório do valor líquido dos itens.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlOpr { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS, referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Parcela correspondente ao valor do ICMS, referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 8, Tamanho = 6)]
    public string? CodObs { get; set; }
}
