using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Generated;

namespace TecnoFisc.Sped.EfdIcmsIpi.Parser;

/// <summary>
/// Leitor especializado do leiaute EFD ICMS-IPI (baseline V015). Resolve o catálogo
/// dos registros declarados em <see cref="TecnoFisc.Sped.EfdIcmsIpi"/> e delega a leitura
/// ao <see cref="LeitorSpedTxt"/> compartilhado pelo Core.
/// </summary>
/// <remarks>
/// A construção sem parâmetros usa o catálogo gerado em tempo de compilação
/// (<see cref="CatalogoSpedGerado"/>). Consumidores de teste ou cenários que substituem o
/// catálogo podem injetar uma implementação alternativa via <see cref="ParserEfdIcmsIpi(IRegistroSpedCatalogo)"/>.
/// </remarks>
public sealed class ParserEfdIcmsIpi : ILeitorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao = new CatalogoSpedGerado();

    private readonly LeitorSpedTxt _leitor;

    /// <summary>Cria o parser usando o catálogo gerado em tempo de compilação.</summary>
    public ParserEfdIcmsIpi() : this(_catalogoPadrao, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com opções de leitura (ex.: ignorar registros/blocos), catálogo gerado.</summary>
    public ParserEfdIcmsIpi(ReadingOptions opcoes) : this(_catalogoPadrao, opcoes)
    {
    }

    /// <summary>Cria o parser com um catálogo customizado (testes, source generator).</summary>
    public ParserEfdIcmsIpi(IRegistroSpedCatalogo catalogo) : this(catalogo, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com catálogo customizado e opções de leitura.</summary>
    public ParserEfdIcmsIpi(IRegistroSpedCatalogo catalogo, ReadingOptions opcoes)
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
    /// Lê o fluxo SPED inteiro e devolve o modelo tipado <see cref="ArquivoEfdIcmsIpi"/> com
    /// todos os blocos populados. Para arquivos muito grandes prefira <see cref="ReadStreamingAsync"/>.
    /// </summary>
    public Task<ArquivoEfdIcmsIpi> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEfdIcmsIpi.LoadAsync(ReadStreamingAsync(entrada, cancelamento), cancelamento);
}
