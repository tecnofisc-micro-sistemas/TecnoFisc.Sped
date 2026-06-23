using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Generated;

namespace TecnoFisc.Sped.Ecd.Parser;

/// <summary>
/// Leitor especializado do leiaute ECD (Escrituração Contábil Digital — leiaute 9). Resolve
/// o catálogo dos registros declarados em <see cref="TecnoFisc.Sped.Ecd"/> e delega a leitura
/// ao <see cref="LeitorSpedTxt"/> compartilhado pelo Core.
/// </summary>
/// <remarks>
/// A construção sem parâmetros usa o catálogo gerado em tempo de compilação
/// (<see cref="CatalogoSpedGerado"/>). Consumidores de teste ou cenários que substituem o
/// catálogo podem injetar uma implementação alternativa via <see cref="ParserEcd(IRegistroSpedCatalogo)"/>.
/// </remarks>
public sealed class ParserEcd : ILeitorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao = new CatalogoSpedGerado();

    private readonly LeitorSpedTxt _leitor;

    /// <summary>Cria o parser usando o catálogo gerado em tempo de compilação.</summary>
    public ParserEcd() : this(_catalogoPadrao, ReadingOptions.Default)
    {
    }

    /// <summary>
    /// Cria o parser com opções de leitura (ex.: ignorar J800/J801 para não materializar o
    /// ARQ_RTF de até 30 MB), usando o catálogo gerado em tempo de compilação.
    /// </summary>
    public ParserEcd(ReadingOptions opcoes) : this(_catalogoPadrao, opcoes)
    {
    }

    /// <summary>Cria o parser com um catálogo customizado (testes, source generator).</summary>
    public ParserEcd(IRegistroSpedCatalogo catalogo) : this(catalogo, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com catálogo customizado e opções de leitura.</summary>
    public ParserEcd(IRegistroSpedCatalogo catalogo, ReadingOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(opcoes);
        _leitor = new LeitorSpedTxt(catalogo, opcoes);
    }

    /// <summary>
    /// Lê o fluxo SPED em modo streaming, materializando um registro por vez sem bufferizar o
    /// arquivo todo. Os registros saem com Pai/Filhos já vinculados.
    /// </summary>
    public IAsyncEnumerable<RegistroSped> ReadStreamingAsync(Stream entrada, CancellationToken cancelamento = default)
        => _leitor.ReadStreamingAsync(entrada, cancelamento);

    /// <summary>
    /// Lê o fluxo SPED inteiro e devolve o modelo tipado <see cref="ArquivoEcd"/> com todos os
    /// blocos populados. Para arquivos muito grandes prefira <see cref="ReadStreamingAsync"/>.
    /// </summary>
    public Task<ArquivoEcd> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEcd.LoadAsync(ReadStreamingAsync(entrada, cancelamento), cancelamento);

    /// <summary>
    /// Parseia uma única linha SPED isoladamente, tolerante por natureza (ver
    /// <see cref="LeitorSpedTxt.ParseLinha"/>). Não constrói hierarquia.
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha);
}
