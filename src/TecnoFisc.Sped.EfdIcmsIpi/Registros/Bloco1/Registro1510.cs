using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1510 - Itens do Documento Nota Fiscal/Conta Energia Eletrica (codigo 06).
/// Nivel hierarquico 3, ocorrencia 1:N por Registro 1500. Conforme Guia Pratico
/// EFD-ICMS/IPI V3.0.6, pp. 285-286.
/// </summary>
[RegistroSped(Codigo = "1510", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1510";

    /// <summary>Numero sequencial do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumItem { get; set; }

    /// <summary>Codigo do item, campo 02 do Registro 0200.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Codigo de classificacao do item de energia eletrica, conforme Tabela 4.4.1.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4, Obrigatorio = true)]
    public int? CodClass { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3)]
    public decimal? Qtd { get; set; }

    /// <summary>Unidade do item, campo 02 do Registro 0190.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Valor do item.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Codigo da Situacao Tributaria referente ao ICMS, conforme Tabela 4.3.1.</summary>
    [CampoSped(Ordem = 9, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Codigo Fiscal de Operacao e Prestacao.</summary>
    [CampoSped(Ordem = 10, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    /// <summary>Valor da base de calculo do ICMS.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Aliquota do ICMS.</summary>
    [CampoSped(Ordem = 12, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor do ICMS creditado/debitado.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor da base de calculo referente a substituicao tributaria.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Aliquota do ICMS da substituicao tributaria na unidade da federacao de destino.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? AliqSt { get; set; }

    /// <summary>Valor do ICMS referente a substituicao tributaria.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Indicador do tipo de receita: 0-Receita propria; 1-Receita de terceiros.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoReceita? IndRec { get; set; }

    /// <summary>Codigo do participante receptor da receita, terceiro da operacao, campo 02 do Registro 0150.</summary>
    [CampoSped(Ordem = 18, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Codigo da conta analitica contabil debitada/creditada.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0)]
    public string? CodCta { get; set; }
}
