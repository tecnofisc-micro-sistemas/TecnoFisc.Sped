using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I555 — Totais no Livro Razão Auxiliar com Leiaute Parametrizável.
/// Nível hierárquico 4, ocorrência 0:N.
/// Informa as chaves utilizadas para totalizações dos campos informados no Registro I510 e
/// os totais. Contém os mesmos campos do Registro I550, mas apenas os campos que serviram
/// de chave para o cálculo dos totais e os campos totalizados são preenchidos — os demais
/// ficam vazios (<c>||</c>).
/// Registro facultativo.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 167–168.
/// </summary>
/// <remarks>
/// O campo <see cref="RzContTot"/> é variádico (<c>*</c> no leiaute): contém N valores
/// pipe-separados correspondendo aos N registros I510 declarados no arquivo. Validações
/// são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_NUM_CAMPOS_RELATORIO</c>: verifica se o número de campos (desconsiderando
///   REG) é igual ao número de registros I510 no arquivo.</item>
///   <item><c>REGRA_TODOS_CAMPOS_VAZIOS</c>: verifica se pelo menos um dos campos declarados
///   no I510 para os registros I550 e I555 foi preenchido.</item>
///   <item><c>REGRA_TIPO_CAMPO_RAZAO_AUXILIAR</c>: verifica se o tipo e as casas decimais de
///   cada valor em <see cref="RzContTot"/> correspondem ao <c>TIPO_CAMPO</c> e
///   <c>DEC_CAMPO</c> do registro I510 de mesma posição.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I555", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI555 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I555";

    /// <summary>
    /// Conteúdo dos campos mencionados no Registro I510, separados por pipe (<c>|</c>).
    /// Campo variádico <c>RZ_CONT_TOT</c> — campos não utilizados como chave ou não
    /// totalizados devem permanecer vazios (<c>||</c>).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, CapturaTudo = true)]
    public string? RzContTot { get; set; }
}
