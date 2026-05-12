using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C610 — Itens do Documento Consolidado: NF/Conta de Energia Elétrica (cód. 06),
/// NF/Conta de Fornecimento D'Água Canalizada (cód. 29) e NF/Conta de Fornecimento de Gás
/// Canalizado (cód. 28), para empresas não obrigadas ao Convênio ICMS 115/2003.
/// Filho do C600. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 146.
/// </summary>
[RegistroSped(Codigo = "C610", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC610 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C610";

    /// <summary>Código de classificação do item de energia elétrica, conforme Tabela 4.4.1. Preenchido apenas quando COD_MOD do C600 for 06.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4)]
    public int? CodClass { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade acumulada do item.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor acumulado do item.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 8, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação, conforme tabela indicada no item 4.2.2.</summary>
    [CampoSped(Ordem = 9, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS debitado.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor da base de cálculo do ICMS substituição tributária.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Valor do ICMS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Valor do PIS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0)]
    public string? CodCta { get; set; }
}
