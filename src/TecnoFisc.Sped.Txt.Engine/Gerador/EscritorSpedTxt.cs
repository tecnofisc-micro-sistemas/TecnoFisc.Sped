using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Txt.Engine.Gerador;

/// <summary>
/// Escritor SPED .txt baseado em <see cref="PipeWriter"/>. Materializa cada registro como
/// uma linha pipe-delimitada terminada por CRLF, codificada em Latin1/Windows-1252.
/// </summary>
/// <remarks>
/// O escritor não injeta totalizadores: os registros de encerramento de bloco (X990) e o
/// encerramento de arquivo (9999) são responsabilidade do montador do formato — tipicamente
/// o <c>TotalizadorBlocos</c>. Aqui apenas grava o que vier no <see cref="IAsyncEnumerable{T}"/>
/// de entrada.
/// </remarks>
public sealed class EscritorSpedTxt : IEscritorSped
{
    private readonly IRegistroSpedCatalogo _catalogo;

    public EscritorSpedTxt(IRegistroSpedCatalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        _catalogo = catalogo;
    }

    /// <inheritdoc />
    public async Task WriteAsync(
        Stream saida,
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(saida);
        ArgumentNullException.ThrowIfNull(registros);

        // leaveOpen: o ciclo de vida do stream é do chamador; o escritor só completa o pipe.
        var escritor = PipeWriter.Create(saida, new StreamPipeWriterOptions(leaveOpen: true));
        try
        {
            await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            {
                EscreverLinha(escritor, registro);

                var resultado = await escritor.FlushAsync(cancelamento).ConfigureAwait(false);
                if (resultado.IsCompleted)
                    break;
            }
        }
        finally
        {
            await escritor.CompleteAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sobrecarga síncrona-like sobre <see cref="IEnumerable{RegistroSped}"/>. Útil quando
    /// o consumidor já tem a coleção em memória e não quer construir um async iterator.
    /// </summary>
    public Task WriteAsync(
        Stream saida,
        IEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(registros);
        return WriteAsync(saida, ParaAsync(registros, cancelamento), cancelamento);
    }

    private static async IAsyncEnumerable<RegistroSped> ParaAsync(
        IEnumerable<RegistroSped> origem,
        [EnumeratorCancellation] CancellationToken cancelamento)
    {
        foreach (var item in origem)
        {
            cancelamento.ThrowIfCancellationRequested();
            yield return item;
        }
        await Task.CompletedTask.ConfigureAwait(false);
    }

    private void EscreverLinha(PipeWriter escritor, RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        if (!_catalogo.TentarObter(registro.Codigo.AsSpan(), out var metadados))
            throw new InvalidOperationException(
                $"Registro com código '{registro.Codigo}' não encontrado no catálogo. " +
                "O escritor só conhece tipos catalogados pela especialização do formato.");

        // Registros descontinuados são catalogados sem campos modelados para que a leitura de
        // arquivos históricos não aborte (ver DescontinuadoAttribute) — mas escrevê-los emitiria
        // só "|CODIGO|", perdendo todo o conteúdo original em silêncio. Falhar alto aqui em vez
        // de produzir uma linha mutilada.
        if (metadados.Campos.Count == 0)
            throw new InvalidOperationException(
                $"Registro com código '{registro.Codigo}' não tem campos modelados no catálogo. " +
                "Escrevê-lo produziria uma linha só com o código, sem os dados originais — não " +
                "suportado. Registros sem campos modelados (tipicamente descontinuados) só são " +
                "suportados para leitura.");

        var construtor = new StringBuilder(capacity: 128);
        construtor.Append('|').Append(metadados.Codigo).Append('|');
        foreach (var campo in metadados.Campos)
        {
            construtor.Append(campo.Serializar(registro));
            construtor.Append('|');
        }
        construtor.Append('\r').Append('\n');

        string linha = construtor.ToString();
        int qtdBytes = EncodingSped.Latin1.GetByteCount(linha);

        var destino = escritor.GetSpan(qtdBytes);
        int gravados = EncodingSped.Latin1.GetBytes(linha, destino);
        escritor.Advance(gravados);
    }
}
