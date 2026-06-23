using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Generated;

namespace TecnoFisc.Sped.EfdContribuicoes.Parser;

/// <summary>
/// Leitor especializado do leiaute EFD Contribuições (V006, guia v1.35). Resolve o catálogo
/// dos registros declarados em <see cref="TecnoFisc.Sped.EfdContribuicoes"/> e delega a leitura
/// ao <see cref="LeitorSpedTxt"/> compartilhado pelo Core.
/// </summary>
/// <remarks>
/// A construção sem parâmetros usa o catálogo gerado em tempo de compilação
/// (<see cref="CatalogoSpedGerado"/>, produzido pelo Stage 6 source generator). Substitui o
/// scan reflexivo de <c>Assembly.GetTypes()</c> que era usado no Stage 2: as entradas do
/// dicionário e as fábricas <c>static () =&gt; new T()</c> existem em compile-time.
/// Consumidores de teste ou cenários que substituem o catálogo podem injetar uma
/// implementação alternativa via <see cref="ParserEfdContribuicoes(IRegistroSpedCatalogo)"/>.
/// </remarks>
public sealed class ParserEfdContribuicoes : ILeitorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao = new CatalogoSpedGerado();

    private readonly LeitorSpedTxt _leitor;

    /// <summary>Cria o parser usando o catálogo gerado em tempo de compilação.</summary>
    public ParserEfdContribuicoes() : this(_catalogoPadrao, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com opções de leitura (ex.: ignorar registros/blocos), catálogo gerado.</summary>
    public ParserEfdContribuicoes(ReadingOptions opcoes) : this(_catalogoPadrao, opcoes)
    {
    }

    /// <summary>Cria o parser com um catálogo customizado (testes, source generator).</summary>
    public ParserEfdContribuicoes(IRegistroSpedCatalogo catalogo) : this(catalogo, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com catálogo customizado e opções de leitura.</summary>
    public ParserEfdContribuicoes(IRegistroSpedCatalogo catalogo, ReadingOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(opcoes);
        _leitor = new LeitorSpedTxt(catalogo, opcoes);
    }

    /// <summary>
    /// Lê o fluxo SPED em modo streaming, materializando um registro por vez sem bufferizar o
    /// arquivo todo. Memória consumida é limitada pelo buffer do <see cref="System.IO.Pipelines.PipeReader"/>
    /// — adequado para arquivos de múltiplos gigabytes. Os registros saem com Pai/Filhos já vinculados.
    /// </summary>
    public IAsyncEnumerable<RegistroSped> ReadStreamingAsync(Stream entrada, CancellationToken cancelamento = default)
        => _leitor.ReadStreamingAsync(entrada, cancelamento);

    /// <summary>
    /// Lê o fluxo SPED inteiro e devolve o modelo tipado <see cref="ArquivoEfdContribuicoes"/> com
    /// todos os blocos populados. Conveniência sobre <see cref="ReadStreamingAsync"/> + <see cref="ArquivoEfdContribuicoes.LoadAsync"/>;
    /// indicado quando o consumidor precisa do arquivo completo em memória. Para arquivos muito
    /// grandes prefira <see cref="ReadStreamingAsync"/>.
    /// </summary>
    public Task<ArquivoEfdContribuicoes> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEfdContribuicoes.LoadAsync(ReadStreamingAsync(entrada, cancelamento), cancelamento);

    /// <summary>
    /// Parseia uma única linha SPED isoladamente, tolerante por natureza (ver
    /// <see cref="LeitorSpedTxt.ParseLinha"/>). Não constrói hierarquia.
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha);
}
