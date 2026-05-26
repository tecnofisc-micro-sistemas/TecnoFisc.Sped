using System.Xml;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Helpers de baixo nível sobre <see cref="XmlReader"/> forward-only que dão suporte à
/// desserialização <b>order-independent</b> dos documentos fiscais (ver
/// <c>sped/STAGE_14_NFE_NFCE.md</c> §3). O contrato uniforme de posicionamento permite compor
/// leitores escalares e complexos sem desserializador posicional.
/// </summary>
internal static class XmlReaderExtensions
{
    /// <summary>
    /// Itera os elementos-filho diretos do elemento em que o <paramref name="reader"/> está
    /// posicionado, despachando cada um por <c>LocalName</c> para <paramref name="dispatch"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>Pré-condição:</b> o reader está posicionado no nó <see cref="XmlNodeType.Element"/>
    /// de início do elemento-pai. <b>Pós-condição:</b> o reader fica posicionado no nó
    /// imediatamente após o <see cref="XmlNodeType.EndElement"/> do pai.</para>
    /// <para><paramref name="dispatch"/> recebe o <c>LocalName</c> do filho e deve devolver
    /// <c>true</c> quando consumiu o elemento por completo — deixando o reader no nó seguinte
    /// (o que <see cref="ReadTextAsync"/> e os leitores complexos fazem). Quando devolve
    /// <c>false</c>, o elemento é tratado como não mapeado: ignorado via
    /// <see cref="XmlReader.SkipAsync"/> (ou, sob <paramref name="strict"/>, interrompe com
    /// <see cref="FormatException"/>).</para>
    /// </remarks>
    public static async ValueTask ConsumeChildrenAsync(
        this XmlReader reader,
        Func<string, ValueTask<bool>> dispatch,
        bool strict,
        CancellationToken cancellationToken)
    {
        // Elemento vazio (<x/>): sem filhos. Avança para o nó seguinte e retorna.
        if (reader.IsEmptyElement)
        {
            await reader.ReadAsync().ConfigureAwait(false);
            return;
        }

        int parentDepth = reader.Depth;

        if (!await reader.ReadAsync().ConfigureAwait(false))
            return; // EOF prematuro: nada a consumir.

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (reader.NodeType)
            {
                case XmlNodeType.EndElement when reader.Depth == parentDepth:
                    await reader.ReadAsync().ConfigureAwait(false); // consome o fim do pai → nó seguinte
                    return;

                case XmlNodeType.Element:
                    string nome = reader.LocalName;
                    if (await dispatch(nome).ConfigureAwait(false))
                        break; // dispatch já deixou o reader no nó seguinte

                    if (strict)
                        throw new FormatException(
                            $"Elemento não mapeado '{nome}' com ParserNFeOptions.Strict ativo.");

                    await reader.SkipAsync().ConfigureAwait(false); // ignora a subárvore → nó seguinte
                    break;

                default:
                    if (!await reader.ReadAsync().ConfigureAwait(false))
                        return; // EOF: encerra defensivamente.
                    break;
            }
        }
    }

    /// <summary>
    /// Lê o conteúdo textual do elemento escalar em que o reader está posicionado e devolve-o
    /// já com <c>Trim()</c> aplicado. Consome o elemento inteiro (início, texto e fim),
    /// posicionando o reader no nó seguinte — exatamente o contrato esperado por
    /// <see cref="ConsumeChildrenAsync"/>.
    /// </summary>
    public static async ValueTask<string> ReadTextAsync(this XmlReader reader)
        => (await reader.ReadElementContentAsStringAsync().ConfigureAwait(false)).Trim();
}
