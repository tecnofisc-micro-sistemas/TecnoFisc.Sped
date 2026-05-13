using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D500 — NF de Serviço de Comunicação (cód. 21) e Serviço de Telecomunicação (cód. 22).
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 192.
/// </summary>
[RegistroSped(Codigo = "D500", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D500";

    /// <summary>Indicador do tipo de operação: 0-Aquisição, 1-Prestação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0-Emissão própria, 1-Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): prestador na aquisição, tomador na prestação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (21 ou 22).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 3)]
    public string? Sub { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 9, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Data da entrada (aquisição) ou da saída (prestação) do serviço no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtAP { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor da prestação de serviços.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor total dos serviços não-tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valores cobrados em nome de terceiros.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    /// <summary>Valor de outras despesas indicadas no documento fiscal.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor do ICMS.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 20, Tamanho = 6)]
    public string? CodInf { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 23, Tamanho = 0)]
    public string? CodCta { get; set; }

    /// <summary>Código do tipo de assinante conforme Tabela 4.4.4.</summary>
    [CampoSped(Ordem = 24, Tamanho = 1)]
    public TipoAssinante? TpAssinante { get; set; }
}
