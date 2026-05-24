namespace TecnoFisc.Sped.Core.Abstracoes;

/// <summary>
/// Leitor de fluxo SPED. Cada projeto de formato fornece uma especialização que conhece
/// o catálogo de registros e devolve a árvore tipada já vinculada.
/// </summary>
public interface ILeitorSped
{
    /// <summary>
    /// Lê o fluxo de entrada e materializa cada registro na ordem em que aparece no arquivo,
    /// em modo streaming — sem bufferizar o arquivo todo. Os registros já vêm com Pai/Filhos
    /// vinculados conforme a pilha hierárquica. Memória consumida pelo enumerador é limitada
    /// pelo buffer do <see cref="System.IO.Pipelines.PipeReader"/> (independente do tamanho do arquivo).
    /// </summary>
    public IAsyncEnumerable<RegistroSped> ReadStreamingAsync(Stream entrada, CancellationToken cancelamento = default);
}
