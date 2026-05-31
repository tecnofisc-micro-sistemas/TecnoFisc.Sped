using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I250 — Partidas do Lançamento. Nível hierárquico 4, ocorrência 0:N por I200.
/// Identifica todas as contrapartidas do lançamento contábil (Registro I200): a soma de todas
/// as partidas a crédito e a soma de todas as partidas a débito devem ser iguais ao valor do
/// lançamento informado no I200 (<c>VL_LCTO</c>).
/// Obrigatório para escrituração do tipo G, R ou A (conforme <c>IND_ESC</c> do Registro I010).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 148–152.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_HISTORICO_OBRIGATORIO</c>: ao menos um dos campos <see cref="CodHistPad"/>
///   (Campo 07) ou <see cref="Hist"/> (Campo 08) deve estar preenchido.</item>
///   <item><c>REGRA_VALIDACAO_VALOR_DEB</c>: a soma dos débitos (por período I150 e conta) é
///   igual ao valor da partida <see cref="VlDc"/> com <see cref="IndDc"/> = D, nos tipos de
///   escrituração G ou R.</item>
///   <item><c>REGRA_VALIDACAO_VALOR_CRED</c>: a soma dos créditos (por período I150 e conta) é
///   igual ao valor da partida <see cref="VlDc"/> com <see cref="IndDc"/> = C, nos tipos de
///   escrituração G ou R.</item>
///   <item><c>REGRA_CONTA_PARA_LANCAMENTO</c>: <see cref="CodCta"/> deve referenciar uma conta
///   analítica existente no Plano de Contas (I050) com <c>IND_CTA = "A"</c>.</item>
///   <item><c>REGRA_CCUS_NO_CENTRO_CUSTOS</c>: <see cref="CodCcus"/> deve existir no Registro
///   I100 (Centro de Custos), quando informado.</item>
///   <item><c>REGRA_COD_HIS_PAD_NO_HISTORICO_PADRAO</c>: <see cref="CodHistPad"/> deve existir
///   na Tabela de Histórico Padronizado (Registro I075), quando informado.</item>
///   <item><c>REGRA_CODIGO_PARTICIPANTE</c>: <see cref="CodPart"/> deve existir na tabela de
///   cadastro de participante (Registro 0150) com vigência válida conforme Registro 0180,
///   quando informado. O PGE gera apenas um aviso quando a regra não é cumprida.</item>
///   <item>Campos adicionais <c>VL_DC_MF</c> (Campo 10) e <c>IND_DC_MF</c> (Campo 11) em moeda
///   funcional são inseridos via Registro I020 quando <c>IDENT_MF = "S"</c> no Registro 0000.
///   Não fazem parte do leiaute fixo — o parser os descarta silenciosamente.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I250", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI250 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I250";

    /// <summary>
    /// Código da conta analítica debitada/creditada.
    /// Campo <c>COD_CTA</c>. Referencia I050 com <c>IND_CTA = "A"</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código do centro de custos.
    /// Campo <c>COD_CCUS</c>. Referencia I100 (<c>REGRA_CCUS_NO_CENTRO_CUSTOS</c>).
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Valor da partida.
    /// Campo <c>VL_DC</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlDc { get; set; }

    /// <summary>
    /// Indicador da natureza da partida: D = Débito; C = Crédito.
    /// Campo <c>IND_DC</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorNaturezaPartida IndDc { get; set; }

    /// <summary>
    /// Número, código ou caminho de localização dos documentos arquivados.
    /// Campo <c>NUM_ARQ</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? NumArq { get; set; }

    /// <summary>
    /// Código do histórico padronizado, conforme tabela I075.
    /// Campo <c>COD_HIST_PAD</c>. Referencia I075 (<c>REGRA_COD_HIS_PAD_NO_HISTORICO_PADRAO</c>).
    /// Ao menos um de <see cref="CodHistPad"/> ou <see cref="Hist"/> deve ser informado
    /// (<c>REGRA_HISTORICO_OBRIGATORIO</c>).
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? CodHistPad { get; set; }

    /// <summary>
    /// Histórico completo da partida ou histórico complementar ao padronizado.
    /// Campo <c>HIST</c>. Quando usado como complemento, combinar com
    /// <c>[DESCR_HIST] do I075 + " " + [HIST]</c>.
    /// Para lançamentos extemporâneos (I200.IND_LCTO = X), deve especificar o motivo da
    /// correção, a data e o número do lançamento de origem (ITG 2000 (R1), item 32).
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 65535)]
    public string? Hist { get; set; }

    /// <summary>
    /// Código de identificação do participante na partida conforme tabela 0150.
    /// Campo <c>COD_PART</c>. Preencher somente quando identificado o tipo de participação
    /// no Registro 0180. Referencia 0150 com vigência válida via 0180
    /// (<c>REGRA_CODIGO_PARTICIPANTE</c> — aviso, não erro).
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? CodPart { get; set; }
}
