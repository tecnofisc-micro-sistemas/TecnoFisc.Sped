using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J150 — Demonstração do Resultado do Exercício (DRE). Nível hierárquico 3, ocorrência 0:N por J005.
/// Informa a DRE da pessoa jurídica a partir dos códigos de aglutinação definidos no Registro I052.
/// Só pode existir uma linha de nível 1 na DRE: o "Resultado do Exercício" (Lucro ou Prejuízo Líquido do Exercício);
/// os demais totalizadores devem estar no nível 2 em diante.
/// Campo(s) chave: [COD_AGL].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 181–186.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_SOMA_NIVEIS_DRE</c>: para totalizadores, o valor total do código de aglutinação deve
///   igualar a soma dos valores dos filhos cujo <see cref="CodAglSup"/> aponta para ele.</item>
///   <item><c>REGRA_NIVEL_1_EXISTENTE</c>: deve existir um registro J150 com <see cref="NivelAgl"/> igual a 1.</item>
///   <item><c>REGRA_OCO_UNICA_NIVEL_1</c>: deve existir exatamente uma linha com <see cref="NivelAgl"/> = 1.</item>
///   <item><c>REGRA_EXISTE_IND_COD_AGLU_DETALHE</c>: deve existir ao menos um J150 com
///   <see cref="IndCodAgl"/> = D (Detalhe).</item>
///   <item><c>REGRA_OBRIGATORIO_I052</c>: para J150 com <see cref="IndCodAgl"/> = D, deve existir
///   registro I052 com o mesmo <see cref="CodAgl"/> e conta analítica no I050.</item>
///   <item><c>REGRA_EXISTE_NOTA_EXPLICATIVA</c>: quando ao menos um J150 tem <see cref="NotaExpRef"/>
///   preenchido, deve existir J800 com TIPO_DOC igual a 010, 011, 012 ou 999.</item>
///   <item><c>REGRA_VALIDA_SALDO_INI_DRE</c>: o <see cref="VlCtaIni"/> deve igualar o saldo recuperado
///   do registro C650 (<c>VL_CTA_FIN</c>) para o mesmo código de aglutinação, quando J005.ID_DEM = 1.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J150", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J150";

    /// <summary>
    /// Número de ordem da linha na visualização da demonstração.
    /// Campo <c>NU_ORDEM</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 19, Obrigatorio = true)]
    public int NuOrdem { get; set; }

    /// <summary>
    /// Código de aglutinação das linhas, atribuído pela pessoa jurídica.
    /// Quando <see cref="IndCodAgl"/> = T (Totalizador), o código deve ser informado mas não precisa
    /// estar cadastrado no Registro I052 (que contém apenas códigos para contas analíticas).
    /// Campo <c>COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodAgl { get; set; }

    /// <summary>
    /// Indicador do tipo de código de aglutinação: T = Totalizador; D = Detalhe.
    /// Campo <c>IND_COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoCodigoAglutinacao IndCodAgl { get; set; }

    /// <summary>
    /// Nível do código de aglutinação (mesmo conceito do plano de contas — Registro I050).
    /// Campo <c>NIVEL_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int NivelAgl { get; set; }

    /// <summary>
    /// Código de aglutinação sintético/grupo de nível superior.
    /// Não preenchido para registros de nível 1.
    /// Campo <c>COD_AGL_SUP</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? CodAglSup { get; set; }

    /// <summary>
    /// Descrição do código de aglutinação.
    /// Campo <c>DESCR_COD_AGL</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? DescrCodAgl { get; set; }

    /// <summary>
    /// Valor do saldo final da linha no período imediatamente anterior (saldo final da DRE anterior).
    /// Campo <c>VL_CTA_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2)]
    public decimal? VlCtaIni { get; set; }

    /// <summary>
    /// Indicador da situação do saldo final do período imediatamente anterior: D = Devedor; C = Credor.
    /// Campo <c>IND_DC_CTA_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 1)]
    public IndicadorDebitoCredito? IndDcCtaIni { get; set; }

    /// <summary>
    /// Valor final da linha antes do encerramento do exercício.
    /// Campo <c>VL_CTA_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCtaFin { get; set; }

    /// <summary>
    /// Indicador da situação do saldo final antes do encerramento do exercício: D = Devedor; C = Credor.
    /// Campo <c>IND_DC_CTA_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDcCtaFin { get; set; }

    /// <summary>
    /// Indicador de grupo da DRE: D = Despesa (redução do lucro); R = Receita (incremento do lucro).
    /// Campo <c>IND_GRP_DRE</c>.
    /// </summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public IndicadorGrupoDre IndGrpDre { get; set; }

    /// <summary>
    /// Referência à numeração das notas explicativas relativas às demonstrações contábeis.
    /// Campo <c>NOTA_EXP_REF</c>.
    /// </summary>
    [CampoSped(Ordem = 13, Tamanho = 12)]
    public string? NotaExpRef { get; set; }
}
