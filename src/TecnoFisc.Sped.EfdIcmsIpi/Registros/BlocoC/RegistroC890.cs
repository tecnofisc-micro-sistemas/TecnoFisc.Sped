using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C890 — Resumo Diário do CF-e-SAT (cód. 59) por Equipamento SAT-CF-e.
/// Consolida os CF-e-SAT emitidos no período por equipamento SAT-CF-e, agrupados
/// por combinação de CST_ICMS, CFOP e alíquota do ICMS.
/// Nível hierárquico 3, ocorrência 1:N por registro C860.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 162.
/// </summary>
[RegistroSped(Codigo = "C890", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC890 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C890";

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
