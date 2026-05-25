using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J210 — DLPA – Demonstração de Lucros ou Prejuízos Acumulados / DMPL – Demonstração de
/// Mutações do Patrimônio Líquido. Nível hierárquico 3, ocorrência 0:N por J005.
/// Informa a DLPA ou a DMPL da pessoa jurídica a partir dos códigos de aglutinação das contas
/// analíticas do patrimônio líquido atribuídos pela empresa.
/// Campo(s) chave: [COD_AGL].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 186–189.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_COD_AGL_DUPLICIDADE</c>: código de aglutinação <see cref="CodAgl"/> não pode
///   ser duplicado dentro do mesmo J005, quando o campo tiver algum conteúdo.</item>
///   <item><c>REGRA_EXISTE_DLPA_OU_DMPL</c>: todos os registros J210 de uma escrituração devem
///   possuir o mesmo valor em <see cref="IndTip"/> por período informado no J005.</item>
///   <item><c>REGRA_UNICO_DLPA</c>: deve existir apenas um J210 quando <see cref="IndTip"/> =
///   <see cref="IndicadorTipoDlpaDmpl.Dlpa"/>; descumprimento gera aviso.</item>
///   <item><c>REGRA_VALIDA_DMPL_COM_SALDO_INI</c> / <c>REGRA_VALIDA_DMPL_COM_SALDO_FIN</c>: quando
///   J005.ID_DEM = 1 e moeda funcional = N (Real), verifica se os saldos inicial/final batem com
///   os valores acumulados do I155 para as datas do J005/I150.</item>
///   <item><c>REGRA_VALIDA_DMPL_COM_SALDO_INI_MF</c> / <c>REGRA_VALIDA_DMPL_COM_SALDO_FIN_MF</c>:
///   mesma verificação quando moeda funcional (Registro0000.IdentMf) = S (Sim).</item>
///   <item><c>REGRA_EXISTE_AGL_J210_MESMO_GRUPO</c>: a natureza da conta (I050.COD_NAT) vinculada ao
///   I052 referenciado por <see cref="CodAgl"/> deve corresponder ao grupo de Patrimônio Líquido.</item>
///   <item><c>REGRA_EXISTE_NOTA_EXPLICATIVA</c>: quando ao menos um J210 tem <see cref="NotasExpRef"/>
///   preenchido, deve existir J800 com TIPO_DOC igual a 010, 011, 012 ou 999.</item>
///   <item><c>REGRA_EXISTE_AGLUTINACAO_J210</c> (campo COD_AGL): deve existir I052 com o mesmo
///   <see cref="CodAgl"/> cuja conta I050 tenha IND_CTA = A (Analítica).</item>
///   <item><c>REGRA_VALIDA_TOT_AGLUTINACAO_J215</c> (campo VL_CTA_FIN): saldo final deve igualar a
///   soma dos valores dos fatos contábeis J215 subtraída do saldo inicial <see cref="VlCtaIni"/>.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J210", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J210";

    /// <summary>
    /// Indicador do tipo de demonstração: 0 = DLPA; 1 = DMPL.
    /// Campo <c>IND_TIP</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoDlpaDmpl IndTip { get; set; }

    /// <summary>
    /// Código de aglutinação das contas analíticas do patrimônio líquido, atribuído pela empresa.
    /// Campo <c>COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodAgl { get; set; }

    /// <summary>
    /// Descrição do código de aglutinação.
    /// Campo <c>DESCR_COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? DescrCodAgl { get; set; }

    /// <summary>
    /// Saldo inicial do código de aglutinação na demonstração do período informado.
    /// Campo <c>VL_CTA_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCtaIni { get; set; }

    /// <summary>
    /// Indicador da situação do saldo inicial: D = Devedor; C = Credor.
    /// Campo <c>IND_DC_CTA_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDcCtaIni { get; set; }

    /// <summary>
    /// Saldo final do código de aglutinação na demonstração do período informado.
    /// Campo <c>VL_CTA_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCtaFin { get; set; }

    /// <summary>
    /// Indicador da situação do saldo final: D = Devedor; C = Credor.
    /// Campo <c>IND_DC_CTA_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDcCtaFin { get; set; }

    /// <summary>
    /// Referência à numeração das notas explicativas relativas às demonstrações contábeis.
    /// Campo <c>NOTAS_EXP_REF</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 12)]
    public string? NotasExpRef { get; set; }
}
