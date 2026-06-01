using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I550 — Detalhes do Livro Razão Auxiliar com Leiaute Parametrizável.
/// Nível hierárquico 3, ocorrência 0:N.
/// Informa o conteúdo dos campos especificados no Registro I510, na mesma ordem,
/// separados por pipe (<c>|</c>). Cada linha I550 corresponde a um item do livro "Z"
/// (Razão Auxiliar com Leiaute Parametrizável).
/// Obrigatório para escriturações do tipo "Z" (<c>IND_ESC = "Z"</c> no Registro I010).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 164–166.
/// </summary>
/// <remarks>
/// O campo <see cref="RzCont"/> é variádico (<c>*</c> no leiaute): contém N valores
/// pipe-separados correspondendo aos N registros I510 declarados no arquivo. O número
/// de valores (desconsiderando o campo REG) deve ser igual ao número de registros I510
/// presentes — validação responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_NUM_CAMPOS_RELATORIO</c>: verifica se o número de campos informados
///   no I550 (desconsiderando REG) é igual ao número de registros I510 no arquivo.</item>
///   <item><c>REGRA_TODOS_CAMPOS_VAZIOS</c>: verifica se pelo menos um dos campos
///   declarados no I510 para os registros I550 e I555 foi preenchido.</item>
///   <item><c>REGRA_TIPO_CAMPO_RAZAO_AUXILIAR</c>: verifica se o tipo e as casas decimais
///   de cada valor em <see cref="RzCont"/> correspondem ao <c>TIPO_CAMPO</c> e
///   <c>DEC_CAMPO</c> do registro I510 de mesma posição.</item>
///   <item><c>REGRA_VALIDA_CONTEUDO_I550_LIVRO_RAS</c>: aplica validações adicionais
///   quando o livro é identificado como "RAZAO_AUXILIAR_DAS_SUBCONTAS" ou
///   "RAZAO_AUXILIAR_DAS_SUBCONTAS_MF" (ver sub-regras no manual, p. 164–165).</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I550", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI550 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I550";

    /// <summary>
    /// Conteúdo dos campos mencionados no Registro I510, separados por pipe (<c>|</c>).
    /// Campo variádico <c>RZ_CONT</c> — o número de valores corresponde ao número de
    /// registros I510 presentes no arquivo. Campos vazios são representados por sequências
    /// de pipes consecutivos (<c>||</c>).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, CapturaTudo = true)]
    public string? RzCont { get; set; }
}
