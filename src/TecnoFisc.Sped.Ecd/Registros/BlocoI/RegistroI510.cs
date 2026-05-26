using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I510 — Definição de Campos do Livro Razão Auxiliar com Leiaute Parametrizável.
/// Nível hierárquico 3, ocorrência 0:N.
/// Define os campos que serão utilizados no livro "Z" (Razão Auxiliar com Leiaute
/// Parametrizável), na mesma ordem em que devem figurar na visualização/impressão.
/// O conteúdo dos campos especificados aqui é informado no Registro I550.
/// Obrigatório para escriturações do tipo "Z" (<c>IND_ESC = "Z"</c> no Registro I010).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 162–163.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_COLUNAS_PAGINA</c>: o somatório de <see cref="ColCampo"/> de todos os
///   registros I510 + número de registros I510 − 1 deve ser igual ao número máximo de
///   caracteres por linha do livro impresso em papel A-4, orientação paisagem, margens
///   de 1,5 cm, fonte Courier, com tamanho definido em <see cref="RegistroI500.TamFonte"/>.</item>
///   <item><c>REGRA_VALIDA_CONTEUDO_I510_LIVRO_RAS</c>: verifica se a natureza do livro
///   (<c>NAT_LIVR</c> do Registro I030) o identifica como "RAZAO_AUXILIAR_DAS_SUBCONTAS"
///   ou "RAZAO_AUXILIAR_DAS_SUBCONTAS_MF" e aplica a validação da estrutura desse livro.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I510", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I510";

    /// <summary>
    /// Nome do campo, sem espaços em branco ou caractere especial.
    /// Campo <c>NM_CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 16, Obrigatorio = true)]
    public string? NmCampo { get; set; }

    /// <summary>
    /// Descrição do campo, utilizada na visualização do Livro Auxiliar.
    /// Campo <c>DESC_CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 50, Obrigatorio = true)]
    public string? DescCampo { get; set; }

    /// <summary>
    /// Tipo do campo: N = Numérico; C = Caractere.
    /// Campo <c>TIPO_CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public TipoCampoRazaoAuxiliar TipoCampo { get; set; }

    /// <summary>
    /// Tamanho do campo.
    /// Campo <c>TAM_CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public int TamCampo { get; set; }

    /// <summary>
    /// Quantidade de casas decimais, aplicável somente para campos do tipo "N".
    /// Campo <c>DEC_CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 2)]
    public int? DecCampo { get; set; }

    /// <summary>
    /// Largura da coluna no relatório, em quantidade de caracteres.
    /// Deve considerar os separadores de milhar e a vírgula para campos numéricos.
    /// Campo <c>COL_CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 3, Obrigatorio = true)]
    public int ColCampo { get; set; }
}
