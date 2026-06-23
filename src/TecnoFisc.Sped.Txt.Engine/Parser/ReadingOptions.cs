namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Opções de leitura aplicadas pelo <see cref="LeitorSpedTxt"/> (e pelos parsers de cada formato).
/// Permite ao consumidor descartar registros indesejados <b>antes</b> da materialização — útil para
/// pular registros pesados como J800/J801 da ECD (campo-arquivo RTF de até 30 MB), evitando o custo
/// de decodificar e alocar o conteúdo.
/// </summary>
/// <remarks>
/// O descarte acontece em nível de byte no leitor: registros ignorados não são decodificados nem
/// devolvidos no stream e não entram na hierarquia Pai/Filhos. Como consequência, contagens do
/// <c>9900</c>/<c>9990</c> e validações de hash do consumidor podem não fechar — é escolha de quem lê.
/// </remarks>
public sealed class ReadingOptions
{
    /// <summary>Instância padrão sem nenhum filtro (lê tudo).</summary>
    public static ReadingOptions Default { get; } = new();

    /// <summary>
    /// Códigos de registro a ignorar (ex.: <c>"J800"</c>, <c>"J801"</c>). Um registro ignorado é
    /// descartado <b>com toda a sua subárvore</b>: registros seguintes de nível hierárquico maior
    /// (filhos/netos) também são descartados, até o próximo registro de nível menor ou igual.
    /// </summary>
    public IReadOnlySet<string> RegistrosIgnorados { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Identificadores de bloco a ignorar (ex.: <c>"J"</c>, <c>"K"</c>). Todo registro cujo bloco
    /// esteja neste conjunto é descartado — incluindo abertura, detalhe e encerramento do bloco.
    /// </summary>
    public IReadOnlySet<string> BlocosIgnorados { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Quando <c>true</c>, uma falha de conversão de campo (FormatException/ArgumentException/
    /// OverflowException no Definidor) NÃO aborta a leitura: o campo fica no default, o erro é
    /// acumulado em <see cref="Abstracoes.RegistroSped.ErrosDeFormato"/> e o parsing continua.
    /// Padrão: <c>false</c> (lança ErroFormatoSpedException no primeiro erro de campo).
    /// Não afeta erros de LAYOUT (registro desconhecido).
    /// </summary>
    public bool LenientFieldParsing { get; init; }

    /// <summary>
    /// <c>true</c> quando há ao menos um filtro configurado. O leitor usa isto para pular toda a
    /// lógica de descarte (fast-path) quando nada deve ser ignorado.
    /// </summary>
    internal bool HasFilter => RegistrosIgnorados.Count > 0 || BlocosIgnorados.Count > 0;
}
