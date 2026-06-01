using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1500 - Nota Fiscal/Conta de Energia Eletrica (codigo 06) - operacoes interestaduais.
/// Nivel hierarquico 2, ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, pp. 282-284.
/// </summary>
[RegistroSped(Codigo = "1500", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1500";

    /// <summary>Indicador do tipo de operacao: 1 - Saida.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao? IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0 - Emissao propria.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento? IndEmit { get; set; }

    /// <summary>Codigo do participante adquirente, campo 02 do Registro 0150.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Codigo do modelo do documento fiscal, conforme Tabela 4.1.1. Valor valido: 06.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Codigo da situacao do documento fiscal, conforme Tabela 4.1.2.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal? CodSit { get; set; }

    /// <summary>Serie do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subserie do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Codigo de classe de consumo de energia eletrica.</summary>
    [CampoSped(Ordem = 9, Tamanho = 2, Obrigatorio = true)]
    public string? CodCons { get; set; }

    /// <summary>Numero do documento fiscal.</summary>
    [CampoSped(Ordem = 10, Tamanho = 9, Obrigatorio = true)]
    public long? NumDoc { get; set; }

    /// <summary>Data da emissao do documento fiscal, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtDoc { get; set; }

    /// <summary>Data da entrada ou da saida, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 12, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtES { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlDoc { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor total fornecido ou consumido.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlForn { get; set; }

    /// <summary>Valor total dos servicos nao tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valor total cobrado em nome de terceiros.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    /// <summary>Valor total de despesas acessorias indicadas no documento fiscal.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    /// <summary>Valor acumulado da base de calculo do ICMS.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor acumulado da base de calculo do ICMS substituicao tributaria.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Valor acumulado do ICMS retido por substituicao tributaria.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Codigo da informacao complementar do documento fiscal, campo 02 do Registro 0450.</summary>
    [CampoSped(Ordem = 23, Tamanho = 6)]
    public string? CodInf { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 24, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 25, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Codigo de tipo de ligacao eletrica: 1-Monofasico; 2-Bifasico; 3-Trifasico.</summary>
    [CampoSped(Ordem = 26, Tamanho = 1)]
    public TipoLigacaoEletrica? TpLigacao { get; set; }

    /// <summary>Codigo de grupo de tensao: 01 a 14.</summary>
    [CampoSped(Ordem = 27, Tamanho = 2)]
    public string? CodGrupoTensao { get; set; }
}
