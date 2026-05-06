using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C100 — Documento - Nota Fiscal (01), Avulsa (1B), Produtor (04), NF-e (55) e NFC-e
/// (65). Nível hierárquico 3, ocorrência 1:N. Conforme Guia Prático v1.35, p. 108.
/// </summary>
[RegistroSped(Codigo = "C100", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC100 : RegistroSped
{
    public override string Codigo => "C100";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoDocumento IndOper { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissaoDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (01, 1B, 04, 55, 65).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 3)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 9, Obrigatorio = true)]
    public string? NumDoc { get; set; }

    /// <summary>Chave da NF-e ou NFC-e (44 dígitos). Obrigatório para COD_MOD 55 de emissão própria.</summary>
    [CampoSped(Ordem = 9, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtES { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorPagamento IndPgto { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlAbatNt { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlMerc { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorFrete IndFrt { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrt { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlSeg { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlOutDa { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    [CampoSped(Ordem = 23, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    [CampoSped(Ordem = 24, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    [CampoSped(Ordem = 25, Tamanho = 0, Decimais = 2)]
    public decimal? VlIpi { get; set; }

    [CampoSped(Ordem = 26, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 27, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 28, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisSt { get; set; }

    [CampoSped(Ordem = 29, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsSt { get; set; }
}
