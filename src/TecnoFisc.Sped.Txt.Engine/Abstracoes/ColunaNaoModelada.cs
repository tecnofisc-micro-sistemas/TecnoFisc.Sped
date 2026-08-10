namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>Por que uma coluna presente na linha não virou propriedade do registro.</summary>
public enum MotivoColunaNaoModelada
{
    /// <summary>
    /// A coluna vem depois do último campo declarado no catálogo — leiaute mais novo que o
    /// modelado, ou registro reconhecido sem nenhum campo modelado (ARCHITECTURE §4.7).
    /// </summary>
    AlemDoModelo = 0,

    /// <summary>
    /// O campo existe no catálogo mas foi introduzido em versão posterior à declarada no
    /// <c>0000</c>, então não vigorava no arquivo lido.
    /// </summary>
    PosteriorAVersaoDeclarada = 1,
}

/// <summary>
/// Coluna presente na linha SPED que o modelo tipado não representa. Preserva o valor em bruto
/// para que nenhum dado do arquivo se perca em silêncio.
/// </summary>
/// <param name="Posicao">
/// Posição na nomenclatura do Guia Prático — a mesma numeração de <c>CampoSpedAttribute.Ordem</c>:
/// <c>1</c> é o próprio <c>REG</c> e os campos do leiaute começam em <c>2</c>.
/// </param>
/// <param name="Valor">Conteúdo da coluna, verbatim, sem trim nem conversão.</param>
/// <param name="Motivo">Por que a coluna não foi materializada.</param>
public readonly record struct ColunaNaoModelada(
    int Posicao, string Valor, MotivoColunaNaoModelada Motivo);
