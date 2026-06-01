using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A100 — Documento - Nota Fiscal de Serviço. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 96.
/// </summary>
[RegistroSped(Codigo = "A100", Nivel = 3, Bloco = "A")]
public sealed partial class RegistroA100 : RegistroSped
{
    public override string Codigo => "A100";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoServico IndOper { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissaoDocumento IndEmit { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 20)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 20)]
    public string? Sub { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 60, Obrigatorio = true)]
    public string? NumDoc { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 60)]
    public string? ChvNfse { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtExeServ { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorPagamento IndPgto { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlPis { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCofins { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofins { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisRet { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsRet { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlIss { get; set; }
}
