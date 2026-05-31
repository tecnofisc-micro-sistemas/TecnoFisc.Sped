using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C500 — NF/Conta de Energia Elétrica (cód. 06), NF de Energia Elétrica Eletrônica
/// – NF3e (cód. 66), NF/Conta de Fornecimento D'Água Canalizada (cód. 29) e NF/Conta de
/// Fornecimento de Gás Canalizado (cód. 28).
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 133-137.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 itens 12-13):</b> validação alterada nos campos 13 <c>VL_DOC</c>,
/// 15 <c>VL_FORN</c> e 30 <c>CHV_DOCe_REF</c>; nova orientação de preenchimento para os
/// campos 12 <c>DT_E_S</c>, 16 <c>VL_SERV_NT</c>, 17 <c>VL_TERC</c>, 20 <c>VL_ICMS</c>
/// e 22 <c>VL_ICMS_ST</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// <para>
/// <b>V017 (Guide 3.1.4 item 2):</b> nova orientação determinando que NF3-e (<c>COD_MOD</c>
/// = "66") sem CST e sem energia injetada não deve ser escriturada neste registro.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "C500", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C500";

    /// <summary>Indicador do tipo de operação: 0 – Entrada; 1 – Saída.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao? IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0 – Emissão própria; 1 – Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento? IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): do adquirente nas saídas; do fornecedor nas entradas.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 06, 28, 29, 66.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal, conforme Tabela 4.1.2. Valores válidos: 00, 01, 02, 03, 06, 07, 08.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal? CodSit { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal. Não informar quando COD_MOD = "66".</summary>
    [CampoSped(Ordem = 8, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>
    /// Código de classe de consumo de energia elétrica ou gás (modelos 06 e 28): 01-Comercial;
    /// 02-Consumo Próprio; 03-Iluminação Pública; 04-Industrial; 05-Poder Público;
    /// 06-Residencial; 07-Rural; 08-Serviço Público. Para água canalizada (29): conforme Tabela 4.4.2.
    /// Não informar para NF3e (66).
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? CodCons { get; set; }

    /// <summary>Número do documento fiscal. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Obrigatorio = true)]
    public long? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtDoc { get; set; }

    /// <summary>Data da entrada ou da saída, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 12, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtES { get; set; }

    /// <summary>Valor total do documento fiscal. Deve ser maior ou igual a zero.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlDoc { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor total fornecido/consumido. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlForn { get; set; }

    /// <summary>Valor total dos serviços não-tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valor total cobrado em nome de terceiros.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    /// <summary>Valor total de despesas acessórias indicadas no documento fiscal.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS substituição tributária.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Valor acumulado do ICMS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 23, Tamanho = 0)]
    public string? CodInf { get; set; }

    /// <summary>Valor do PIS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 24, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 25, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>
    /// Código de tipo de ligação elétrica: 1-Monofásico; 2-Bifásico; 3-Trifásico.
    /// Obrigatório nas saídas com COD_MOD = "06". Não preencher para COD_MOD = "66".
    /// </summary>
    [CampoSped(Ordem = 26, Tamanho = 1)]
    public TipoLigacaoEletrica? TpLigacao { get; set; }

    /// <summary>
    /// Código de grupo de tensão (ANEEL): 01=A1 (≥230kV) … 14=B4b (Iluminação Pública-bulbo de lâmpada).
    /// Obrigatório nas saídas com COD_MOD = "06". Não preencher para COD_MOD = "66".
    /// </summary>
    [CampoSped(Ordem = 27, Tamanho = 2)]
    public string? CodGrupoTensao { get; set; }

    /// <summary>
    /// Chave da Nota Fiscal de Energia Elétrica Eletrônica (NF3e), 44 dígitos.
    /// Obrigatório a partir de 01/01/2020 quando COD_MOD = "66". O DV é conferido.
    /// </summary>
    [CampoSped(Ordem = 28, Tamanho = 44)]
    public string? ChvDoce { get; set; }

    /// <summary>Finalidade da emissão do documento eletrônico. Informar somente quando COD_MOD = "66".</summary>
    [CampoSped(Ordem = 29, Tamanho = 1)]
    public FinalidadeDocumentoEletronico? FinDoce { get; set; }

    /// <summary>
    /// Chave da nota referenciada (NF3e), 44 dígitos. Preencher somente quando COD_MOD = "66"
    /// e FIN_DOCe = 2 (Substituição).
    /// </summary>
    [CampoSped(Ordem = 30, Tamanho = 44)]
    public string? ChvDoceRef { get; set; }

    /// <summary>
    /// Indicador do destinatário/acessante. Obrigatório nas saídas; não apresentar nas entradas.
    /// 1-Contribuinte do ICMS; 2-Contribuinte Isento de Inscrição; 9-Não Contribuinte.
    /// </summary>
    [CampoSped(Ordem = 31, Tamanho = 1)]
    public IndicadorDestinatario? IndDest { get; set; }

    /// <summary>Código do município do destinatário conforme tabela do IBGE (7 dígitos). Obrigatório nas saídas.</summary>
    [CampoSped(Ordem = 32, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 33, Tamanho = 0)]
    public string? CodCta { get; set; }

    /// <summary>Código do modelo do documento fiscal referenciado, conforme a Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 34, Tamanho = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public string? CodModDocRef { get; set; }

    /// <summary>Código de autenticação digital do registro (Convênio 115/2003).</summary>
    [CampoSped(Ordem = 35, Tamanho = 32, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public string? HashDocRef { get; set; }

    /// <summary>Série do documento fiscal referenciado.</summary>
    [CampoSped(Ordem = 36, Tamanho = 4, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public string? SerDocRef { get; set; }

    /// <summary>Número do documento fiscal referenciado.</summary>
    [CampoSped(Ordem = 37, Tamanho = 9, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public int? NumDocRef { get; set; }

    /// <summary>Mês e ano da emissão do documento fiscal referenciado (formato MMAAAA).</summary>
    [CampoSped(Ordem = 38, Tamanho = 6, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public string? MesDocRef { get; set; }

    /// <summary>Energia injetada.</summary>
    [CampoSped(Ordem = 39, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public decimal? EnerInjet { get; set; }

    /// <summary>Outras deduções.</summary>
    [CampoSped(Ordem = 40, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public decimal? OutrasDed { get; set; }
}
