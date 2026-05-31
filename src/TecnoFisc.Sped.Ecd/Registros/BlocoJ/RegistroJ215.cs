using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J215 — Fato Contábil que Altera a Conta Lucros Acumulados ou a Conta Prejuízos
/// Acumulados ou Todo o Patrimônio Líquido. Nível hierárquico 4, ocorrência 0:N por J210.
/// Informa os fatos contábeis que alteram a conta "Lucros Acumulados", "Prejuízos Acumulados"
/// ou quaisquer outras contas do Patrimônio Líquido. A ordem de apresentação representa a ordem
/// de exibição na DMPL. O primeiro J215 deve conter o saldo inicial do código de aglutinação do J210.
/// Campo(s) chave: [COD_HIST_FAT].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 189–190.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_DUPLICIDADE_HIST_FAT</c>: <see cref="CodHistFat"/> não pode ser duplicado
///   dentro do mesmo arquivo; descumprimento gera aviso.</item>
///   <item><c>REGRA_VALIDA_TOT_AGLUTINACAO_J215</c>: saldo final de J210 (<c>VL_CTA_FIN</c>) deve
///   igualar a soma de todos os <see cref="VlFatCont"/> deste J215 subtraída do saldo inicial
///   de J210 (<c>VL_CTA_INI</c>).</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J215", Nivel = 4, Bloco = "J")]
public sealed partial class RegistroJ215 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J215";

    /// <summary>
    /// Código do histórico do fato contábil.
    /// Campo <c>COD_HIST_FAT</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodHistFat { get; set; }

    /// <summary>
    /// Descrição do fato contábil.
    /// Campo <c>DESC_FAT</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescFat { get; set; }

    /// <summary>
    /// Valor do fato contábil.
    /// Campo <c>VL_FAT_CONT</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlFatCont { get; set; }

    /// <summary>
    /// Indicador de situação do saldo do fato contábil.
    /// D = Devedor; C = Credor; P = Subtotal ou total positivo; N = Subtotal ou total negativo.
    /// Campo <c>IND_DC_FAT</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSituacaoFatoContabil IndDcFat { get; set; }
}
