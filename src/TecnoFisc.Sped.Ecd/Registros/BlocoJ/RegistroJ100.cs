using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J100 — Balanço Patrimonial. Nível hierárquico 3, ocorrência 0:N por J005.
/// Informa o Balanço Patrimonial da pessoa jurídica a partir dos códigos de aglutinação
/// definidos no Registro I052. A estrutura hierárquica é determinada pela sequência de
/// <see cref="NivelAgl"/> e pelo campo <see cref="CodAglSup"/>. Só podem existir duas
/// linhas de nível 1 no balanço: Ativo (A) e Passivo e Patrimônio Líquido (P).
/// Campo(s) chave: [COD_AGL].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 175–179.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_SOMA_DAS_PARCELAS_BALANCOS_INI</c>: saldo inicial do totalizador deve
///   igualar a soma dos saldos iniciais dos filhos (cujo COD_AGL_SUP aponta para ele),
///   considerando <see cref="IndDcCtaIni"/>.</item>
///   <item><c>REGRA_SOMA_DAS_PARCELAS_BALANCOS_FIN</c>: idem para saldos finais.</item>
///   <item><c>REGRA_VALIDA_ATIVO_PASSIVO_INI</c>/<c>REGRA_VALIDA_ATIVO_PASSIVO_FIN</c>:
///   saldo de nível 1 do grupo Ativo deve igualar o saldo de nível 1 do grupo Passivo e PL.</item>
///   <item><c>REGRA_EXISTEM_2_NIVEIS_1</c>: devem existir exatamente duas linhas de nível 1:
///   uma com <see cref="IndGrpBal"/> = A e outra com P.</item>
///   <item><c>REGRA_EXISTE_IND_COD_AGLU_DETALHE</c>: deve existir ao menos um J100 com
///   <see cref="IndCodAgl"/> = D (Detalhe).</item>
///   <item><c>REGRA_OBRIGATORIO_I052</c>: para J100 com <see cref="IndCodAgl"/> = D, deve
///   existir registro I052 com o mesmo <see cref="CodAgl"/> e conta analítica no I050.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J100", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J100";

    /// <summary>
    /// Código de aglutinação atribuído pela pessoa jurídica.
    /// Campo <c>COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodAgl { get; set; }

    /// <summary>
    /// Indicador do tipo de código de aglutinação: T = Totalizador; D = Detalhe.
    /// Campo <c>IND_COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoCodigoAglutinacao IndCodAgl { get; set; }

    /// <summary>
    /// Nível do código de aglutinação (mesmo conceito do plano de contas — Registro I050).
    /// Campo <c>NIVEL_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public int NivelAgl { get; set; }

    /// <summary>
    /// Código de aglutinação sintético/grupo de nível superior.
    /// Não preenchido para registros de nível 1.
    /// Campo <c>COD_AGL_SUP</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? CodAglSup { get; set; }

    /// <summary>
    /// Indicador de grupo do balanço: A = Ativo; P = Passivo e Patrimônio Líquido.
    /// Campo <c>IND_GRP_BAL</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorGrupoBalanco IndGrpBal { get; set; }

    /// <summary>
    /// Descrição do código de aglutinação.
    /// Campo <c>DESCR_COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? DescrCodAgl { get; set; }

    /// <summary>
    /// Valor inicial do código de aglutinação no Balanço Patrimonial no exercício informado.
    /// Campo <c>VL_CTA_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCtaIni { get; set; }

    /// <summary>
    /// Indicador da situação do saldo inicial: D = Devedor; C = Credor.
    /// Campo <c>IND_DC_CTA_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDcCtaIni { get; set; }

    /// <summary>
    /// Valor final do código de aglutinação no Balanço Patrimonial no exercício informado.
    /// Campo <c>VL_CTA_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCtaFin { get; set; }

    /// <summary>
    /// Indicador da situação do saldo final: D = Devedor; C = Credor.
    /// Campo <c>IND_DC_CTA_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDcCtaFin { get; set; }

    /// <summary>
    /// Referência à numeração das notas explicativas relativas às demonstrações contábeis.
    /// Campo <c>NOTA_EXP_REF</c>.
    /// </summary>
    [CampoSped(Ordem = 12, Tamanho = 12)]
    public string? NotaExpRef { get; set; }
}
