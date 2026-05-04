namespace TecnoFisc.Sped.Core.Abstracoes;

/// <summary>
/// Escritor de arquivo SPED. Recebe a sequência de registros já vinculados (ou em ordem
/// linear de gravação) e materializa cada linha no fluxo de saída. Cada projeto de
/// formato fornece a especialização que conhece o catálogo e o totalizador de blocos.
/// </summary>
public interface IEscritorSped
{
    /// <summary>
    /// Escreve cada registro na ordem em que vem do <paramref name="registros"/>. O fluxo
    /// não é fechado pelo escritor — o chamador controla seu ciclo de vida.
    /// </summary>
    public Task EscreverAsync(
        Stream saida,
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default);
}
