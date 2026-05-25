using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I200 — Lançamento Contábil. Nível hierárquico 3, ocorrência 0:N por I150.
/// Define o cabeçalho do lançamento contábil. Três tipos de lançamento são suportados:
/// N (normal), E (encerramento de contas de resultado) e X (extemporâneo, conforme ITG 2000 (R1)).
/// Obrigatório para escrituração do tipo G, R ou A (conforme <c>IND_ESC</c> do Registro I010).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 142–147.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_VALIDACAO_SALDO_CONTA</c>: para cada data de encerramento (I350.DT_RES) e conta,
///   a soma dos lançamentos tipo E com o indicador invertido deve ser igual ao saldo final antes do
///   encerramento (I355.VL_CTA), para escriturações G ou R.</item>
///   <item><c>REGRA_LCTO_4_FORMULA</c>: lançamentos normais (IND_LCTO = "N") devem ter ao menos
///   2 partidas crédito e 2 débito no I250, caso contrário o PGE gera aviso.</item>
///   <item><c>REGRA_REGISTRO_OBRIGATORIO_I350</c>: se a data de encerramento foi informada no I350,
///   deve haver ao menos um lançamento de encerramento (IND_LCTO = "E") para escriturações G ou R.</item>
///   <item><c>REGRA_REGISTRO_DUPLICADO</c>: <see cref="NumLcto"/> é chave única do lançamento.</item>
///   <item><c>REGRA_DT_LCTO_EXT_OBRIGATORIA</c>: <see cref="DtLctoExt"/> é obrigatório quando
///   <see cref="IndLcto"/> = X (extemporâneo). Quando não for possível precisar a data, informar
///   a data de encerramento do exercício em que ocorreram os fatos.</item>
///   <item><c>REGRA_DT_LCTO_EXT_INDEVIDA</c>: <see cref="DtLctoExt"/> não deve ser preenchido
///   quando <see cref="IndLcto"/> ≠ X.</item>
///   <item>Campo adicional <c>VL_LCTO_MF</c> (moeda funcional) é gerado via Registro I020 e presente
///   somente quando <c>IDENT_MF = "S"</c> no Registro 0000. Não faz parte do leiaute fixo —
///   o parser o descarta silenciosamente.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I200", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I200";

    /// <summary>
    /// Número ou código de identificação único do lançamento contábil.
    /// Campo <c>NUM_LCTO</c>. Chave do registro (<c>REGRA_REGISTRO_DUPLICADO</c>).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? NumLcto { get; set; }

    /// <summary>
    /// Data do lançamento (ddMMyyyy).
    /// Campo <c>DT_LCTO</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtLcto { get; set; }

    /// <summary>
    /// Valor do lançamento. Corresponde à soma das partidas (I250) com o mesmo indicador D ou C.
    /// Campo <c>VL_LCTO</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlLcto { get; set; }

    /// <summary>
    /// Indicador do tipo de lançamento (N = Normal; E = Encerramento de contas de resultado;
    /// X = Extemporâneo).
    /// Campo <c>IND_LCTO</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoLancamento IndLcto { get; set; }

    /// <summary>
    /// Data de ocorrência dos fatos objeto do lançamento extemporâneo (ddMMyyyy).
    /// Preenchido apenas quando <see cref="IndLcto"/> = X (<c>REGRA_DT_LCTO_EXT_OBRIGATORIA</c>).
    /// Campo <c>DT_LCTO_EXT</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtLctoExt { get; set; }
}
