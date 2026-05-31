using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I012 — Livros Auxiliares ao Diário ou Livro Principal. Nível hierárquico 3,
/// ocorrência 1:N. Identifica os livros auxiliares associados à escrituração principal (quando
/// <c>IND_ESC</c> do I010 = "R" ou "B"), ou o livro principal associado à escrituração auxiliar
/// (quando <c>IND_ESC</c> = "A" ou "Z").
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 105–107.
/// </summary>
/// <remarks>
/// <para>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OCORRENCIA_UNITARIA_I012</c>: ocorrência única por arquivo quando
///   IND_ESC = "A" ou "Z" (apenas um livro principal associado).</item>
///   <item><c>REGRA_MAIOR_QUE_ZERO</c>: NUM_ORD (campo 02) deve ser maior que zero;
///   deve ser sequencial por tipo de livro.</item>
///   <item><c>REGRA_VALIDA_TIPO_LIVRO_AUXILIAR</c>: quando IND_NIRE (campo 13 do
///   Registro 0000) = 1 (possui registro na Junta Comercial), TIPO deve ser 0 — livros
///   auxiliares de empresas registradas devem ser digitais.</item>
///   <item><c>REGRA_CAMPO_COD_HASH_AUX_OBRIGATORIO</c>: COD_HASH_AUX (campo 05) é
///   obrigatório quando IND_ESC = "R" ou "B" e TIPO = 0.</item>
///   <item><c>REGRA_VALIDA_HEXADECIMAL</c>: COD_HASH_AUX deve conter apenas dígitos
///   (0-9) e letras A-F maiúsculas (gera aviso se violado).</item>
/// </list>
/// </para>
/// </remarks>
[RegistroSped(Codigo = "I012", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI012 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I012";

    /// <summary>
    /// Número de ordem do instrumento associado; deve ser sequencial por tipo de livro.
    /// Campo <c>NUM_ORD</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public int NumOrd { get; set; }

    /// <summary>
    /// Natureza do livro associado; finalidade a que se destina o instrumento.
    /// Campo <c>NAT_LIVR</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 80, Obrigatorio = true)]
    public string? NatLivr { get; set; }

    /// <summary>
    /// Tipo de escrituração do livro associado: 0 = digital (incluído no Sped); 1 = outros.
    /// Campo <c>TIPO</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public TipoEscrituracaoLivroAuxiliar Tipo { get; set; }

    /// <summary>
    /// Código <em>hash</em> do arquivo correspondente ao livro auxiliar utilizado na assinatura
    /// digital. Deve conter apenas dígitos (0-9) e letras A-F maiúsculas.
    /// Campo <c>COD_HASH_AUX</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 40)]
    public string? CodHashAux { get; set; }
}
