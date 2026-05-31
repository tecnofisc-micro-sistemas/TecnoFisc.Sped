using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I500 — Parâmetros de Impressão e Visualização do Razão Auxiliar com Leiaute Parametrizável.
/// Nível hierárquico 3, ocorrência 0:1 por arquivo.
/// Especifica o tamanho da fonte para impressão/visualização do livro "Z" (Razão Auxiliar com
/// Leiaute Parametrizável), impresso em papel A4, orientação paisagem, margens de 1,5 cm, fonte Courier.
/// Obrigatório para escriturações do tipo "Z" (<c>IND_ESC = "Z"</c> no Registro I010).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 161.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_TAM_FONTE</c>: o valor de <see cref="TamFonte"/> deve ser maior que 3 e
///   menor que 13. O PGE do Sped Contábil gera um erro se a regra não for cumprida.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I500", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I500";

    /// <summary>
    /// Tamanho da fonte utilizada na impressão/visualização do livro.
    /// Campo <c>TAM_FONTE</c>. Valores válidos: maior que 3 e menor que 13 (<c>REGRA_TAM_FONTE</c>).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int TamFonte { get; set; }
}
