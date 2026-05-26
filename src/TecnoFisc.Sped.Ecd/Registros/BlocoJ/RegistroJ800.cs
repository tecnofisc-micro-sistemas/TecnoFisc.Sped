using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J800 — Outras Informações. Nível hierárquico 3, ocorrência 1:N por J005.
/// Permite anexar um arquivo no formato RTF (Rich Text Format) com limite de 30 MB na
/// escrituração — usado para notas explicativas, demonstrações contábeis adicionais,
/// pareceres, relatórios e outros documentos que devem constar do livro contábil.
/// O conteúdo binário do arquivo RTF é serializado diretamente no campo <c>ARQ_RTF</c>;
/// o campo <c>IND_FIM_RTF</c> (texto fixo "J800FIM") sinaliza o fim do arquivo.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 190–192.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_VALIDA_HASH_ARQUIVO</c>: <see cref="HashRtf"/> deve ser igual ao hash
///   calculado sobre o conteúdo de <see cref="ArqRtf"/>; descumprimento gera erro.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J800", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ800 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J800";

    /// <summary>
    /// Tipo de documento anexado.
    /// Campo <c>TIPO_DOC</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public TipoDocumentoJ800 TipoDoc { get; set; }

    /// <summary>
    /// Descrição do arquivo .rtf.
    /// Campo <c>DESC_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? DescRtf { get; set; }

    /// <summary>
    /// Hash do arquivo .rtf incluído (41 caracteres). Preenchido automaticamente pelo
    /// sistema — não editável nem alterável pelo declarante.
    /// Campo <c>HASH_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 41)]
    public string? HashRtf { get; set; }

    /// <summary>
    /// Sequência de bytes que representam o arquivo no formato RTF (Rich Text Format),
    /// com limite de 30 MB. Serializado diretamente no campo SPED sem codificação adicional.
    /// Campo <c>ARQ_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? ArqRtf { get; set; }

    /// <summary>
    /// Indicador de fim do arquivo RTF. Texto fixo contendo "J800FIM".
    /// Campo <c>IND_FIM_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 7, Obrigatorio = true)]
    public string? IndFimRtf { get; set; }
}
