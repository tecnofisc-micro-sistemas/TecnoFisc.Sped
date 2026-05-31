using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D500 — Nota Fiscal de Serviço de Comunicação (Código 21) e Nota Fiscal de Serviço de
/// Telecomunicação (Código 22) – Documentos de Aquisição com Direito a Crédito.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 215.
/// </summary>
[RegistroSped(Codigo = "D500", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD500 : RegistroSped
{
    public override string Codigo => "D500";

    /// <summary>Indicador do tipo de operação: 0 – Aquisição.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoDocumento IndOper { get; set; }

    /// <summary>Indicador do emitente: 0 – Emissão própria; 1 – Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissaoDocumento IndEmit { get; set; }

    /// <summary>Código do participante prestador do serviço (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 21, 22.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal, conforme Tabela 4.1.2. Valores válidos: 00, 01, 02, 03, 06, 07, 08.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 3)]
    public string? Sub { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 9, Obrigatorio = true)]
    public string? NumDoc { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAP { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 20, Tamanho = 6)]
    public string? CodInf { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
