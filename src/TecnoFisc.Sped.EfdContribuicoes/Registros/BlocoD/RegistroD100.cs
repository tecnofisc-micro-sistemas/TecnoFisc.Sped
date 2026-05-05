using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D100 — Aquisição de Serviços de Transporte (Códigos 07, 08, 8B, 09, 10, 11, 26, 27,
/// 57, 63 e 67). Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 194.
/// </summary>
[RegistroSped(Codigo = "D100", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD100 : RegistroSped
{
    public override string Codigo => "D100";

    /// <summary>Indicador do tipo de operação: 0 – Aquisição.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoDocumento IndOper { get; set; }

    /// <summary>Indicador do emitente: 0 – Emissão Própria; 1 – Emissão por Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissaoDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): transportador ou pessoa física subcontratada.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (07, 08, 8B, 09, 10, 11, 26, 27, 57, 63, 67).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2 (00, 02, 04, 05, 06, 08).</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 3)]
    public string? Sub { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 9, Obrigatorio = true)]
    public string? NumDoc { get; set; }

    /// <summary>Chave do CT-e (44 dígitos). Obrigatório para COD_MOD 57 a partir de abril de 2012.</summary>
    [CampoSped(Ordem = 10, Tamanho = 44)]
    public ChaveAcesso? ChvCte { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtAP { get; set; }

    /// <summary>Tipo de CT-e conforme Manual de Integração do CT-e. Preencher quando COD_MOD = 57.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1)]
    public int? TpCTe { get; set; }

    /// <summary>Chave do CT-e de referência cujos valores foram complementados (TP_CT-e = 1) ou anulados (TP_CT-e = 2).</summary>
    [CampoSped(Ordem = 14, Tamanho = 44)]
    public ChaveAcesso? ChvCteRef { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Indicador do tipo do frete: 0 – Por conta do emitente; 1 – Por conta do destinatário/remetente; 2 – Por conta de terceiros; 9 – Sem frete.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorFrete IndFrt { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlNt { get; set; }

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 22, Tamanho = 6)]
    public string? CodInf { get; set; }

    [CampoSped(Ordem = 23, Tamanho = 255)]
    public string? CodCta { get; set; }
}
