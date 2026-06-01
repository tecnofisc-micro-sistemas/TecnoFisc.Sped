using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C510 — Itens do Documento NF/Conta de Energia Elétrica (cód. 06),
/// NF/Conta de Fornecimento D'Água Canalizada (cód. 29) e NF/Conta de Fornecimento
/// de Gás Canalizado (cód. 28).
/// Nível hierárquico 3, ocorrência 1:N (por registro C500).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 137-138.
/// </summary>
[RegistroSped(Codigo = "C510", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C510";

    /// <summary>Número sequencial do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumItem { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Código de classificação do item de energia elétrica, conforme Tabela 4.4.1.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public int? CodClass { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3)]
    public decimal? Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 6, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Valor do item.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela 4.3.1.</summary>
    [CampoSped(Ordem = 9, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação.</summary>
    [CampoSped(Ordem = 10, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 12, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor do ICMS creditado/debitado.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor da base de cálculo referente à substituição tributária.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Alíquota do ICMS da substituição tributária na UF de destino.</summary>
    [CampoSped(Ordem = 15, Tamanho = 6, Decimais = 2)]
    public decimal? AliqSt { get; set; }

    /// <summary>Valor do ICMS referente à substituição tributária.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Indicador do tipo de receita: 0-Receita própria; 1-Receita de terceiros.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoReceita? IndRec { get; set; }

    /// <summary>Código do participante receptor da receita, terceiro da operação (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 18, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Valor do PIS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0)]
    public string? CodCta { get; set; }
}
