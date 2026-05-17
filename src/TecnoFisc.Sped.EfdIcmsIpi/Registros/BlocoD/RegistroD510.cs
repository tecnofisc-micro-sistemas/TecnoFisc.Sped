using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D510 — Itens do Documento: NF de Serviço de Comunicação (cód. 21)
/// e Serviço de Telecomunicação (cód. 22).
/// Nível hierárquico 3, ocorrência 1:N (por registro D500).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 193-194.
/// </summary>
[RegistroSped(Codigo = "D510", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D510";

    /// <summary>Número sequencial do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumItem { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Código de classificação do item do serviço de comunicação ou telecomunicação, conforme Tabela 4.4.1.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4, Obrigatorio = true)]
    public int? CodClass { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal? Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor do item.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
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

    /// <summary>Valor da base de cálculo do ICMS de outras UFs.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsUf { get; set; }

    /// <summary>Valor do ICMS de outras UFs.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsUf { get; set; }

    /// <summary>Indicador do tipo de receita.</summary>
    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoReceitaTelecomunicacao? IndRec { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150) receptor da receita, terceiro da operação, se houver.</summary>
    [CampoSped(Ordem = 17, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0)]
    public string? CodCta { get; set; }
}
