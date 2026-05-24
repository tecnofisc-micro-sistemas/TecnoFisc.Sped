using System.Runtime.CompilerServices;
using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Streaming;

/// <summary>
/// Extensões idiomáticas sobre o <see cref="IAsyncEnumerable{T}"/> produzido pelos parsers SPED.
/// Facilitam o caso de uso comum de ingestão para banco de dados: filtro por tipo concreto de
/// registro (<see cref="OfType{T}"/>) e agrupamento em lotes para bulk-insert (<see cref="Batch{T}"/>).
/// </summary>
public static class StreamingExtensions
{
    /// <summary>
    /// Filtra o stream mantendo apenas registros do tipo concreto <typeparamref name="T"/>.
    /// Equivalente assíncrono ao <see cref="System.Linq.Enumerable.OfType{TResult}"/> do LINQ,
    /// porém especializado para a hierarquia de <see cref="RegistroSped"/>. O cast é resolvido
    /// em compile-time via pattern matching — zero reflection, zero boxing.
    /// </summary>
    /// <typeparam name="T">Tipo concreto de registro (ex.: <c>RegistroC100</c>).</typeparam>
    /// <param name="origem">Stream produzido por <c>ParserXxx.ReadStreamingAsync</c>.</param>
    /// <param name="cancellationToken">Token de cancelamento propagado ao enumerador subjacente.</param>
    /// <example>
    /// <code>
    /// await foreach (var c100 in parser.ReadStreamingAsync(stream).OfType&lt;RegistroC100&gt;())
    /// {
    ///     // c100 já tipado, sem cast
    /// }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<T> OfType<T>(
        this IAsyncEnumerable<RegistroSped> origem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : RegistroSped
    {
        ArgumentNullException.ThrowIfNull(origem);

        await foreach (var registro in origem.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (registro is T typed)
            {
                yield return typed;
            }
        }
    }

    /// <summary>
    /// Agrupa o stream em lotes de tamanho <paramref name="size"/>. O último lote pode ser menor
    /// se a quantidade total de itens não for múltiplo exato. Ideal para bulk-insert em banco
    /// (EF Core <c>AddRangeAsync</c>, Dapper, <c>SqlBulkCopy</c>): persistir lote a lote
    /// reduz round-trips em 10-100x comparado a INSERT-por-registro.
    /// </summary>
    /// <typeparam name="T">Tipo dos itens (registro tipado ou DTO já projetado).</typeparam>
    /// <param name="origem">Stream a agrupar.</param>
    /// <param name="size">Tamanho do lote. Deve ser maior que zero.</param>
    /// <param name="cancellationToken">Token de cancelamento propagado ao enumerador subjacente.</param>
    /// <exception cref="ArgumentNullException"><paramref name="origem"/> é <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> não é positivo.</exception>
    /// <example>
    /// <code>
    /// await foreach (var lote in parser.ReadStreamingAsync(stream)
    ///     .OfType&lt;RegistroC100&gt;()
    ///     .Batch(1000))
    /// {
    ///     await conexao.BulkInsertAsync(lote);
    /// }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<IReadOnlyList<T>> Batch<T>(
        this IAsyncEnumerable<T> origem,
        int size,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(origem);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        var buffer = new List<T>(size);
        await foreach (var item in origem.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            buffer.Add(item);
            if (buffer.Count == size)
            {
                yield return buffer;
                buffer = new List<T>(size);
            }
        }

        if (buffer.Count > 0)
        {
            yield return buffer;
        }
    }
}
