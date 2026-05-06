using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Parser;

/// <summary>
/// Leitor especializado do leiaute EFD Contribuições (V006, guia v1.35). Resolve o catálogo
/// dos registros declarados em <see cref="TecnoFisc.Sped.EfdContribuicoes"/> e delega a leitura
/// ao <see cref="LeitorSpedTxt"/> compartilhado pelo Core.
/// </summary>
/// <remarks>
/// A construção sem parâmetros usa o catálogo padrão do assembly de EFD Contribuições, montado
/// uma única vez via <see cref="CatalogoBuilder"/> (cache global por assembly). Consumidores de
/// teste ou cenários que substituem o catálogo (ex.: Stage 6, source generator) podem injetar
/// uma implementação alternativa.
/// </remarks>
public sealed class ParserEfdContribuicoes : ILeitorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao =
        CatalogoBuilder.BuildFromAssembly(typeof(ParserEfdContribuicoes).Assembly);

    private readonly LeitorSpedTxt _leitor;

    /// <summary>Cria o parser usando o catálogo padrão do assembly de EFD Contribuições.</summary>
    public ParserEfdContribuicoes() : this(_catalogoPadrao)
    {
    }

    /// <summary>Cria o parser com um catálogo customizado (testes, source generator).</summary>
    public ParserEfdContribuicoes(IRegistroSpedCatalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        _leitor = new LeitorSpedTxt(catalogo);
    }

    /// <summary>
    /// Lê o fluxo SPED em modo streaming, materializando um registro por vez sem bufferizar o
    /// arquivo todo. Memória consumida é limitada pelo buffer do <see cref="System.IO.Pipelines.PipeReader"/>
    /// — adequado para arquivos de múltiplos gigabytes. Os registros saem com Pai/Filhos já vinculados.
    /// </summary>
    public IAsyncEnumerable<RegistroSped> LerStreamingAsync(Stream entrada, CancellationToken cancelamento = default)
        => _leitor.LerStreamingAsync(entrada, cancelamento);

    /// <summary>
    /// Lê o fluxo SPED inteiro e devolve o modelo tipado <see cref="ArquivoEfdContribuicoes"/> com
    /// todos os blocos populados. Conveniência sobre <see cref="LerStreamingAsync"/> + <see cref="ArquivoEfdContribuicoes.CarregarAsync"/>;
    /// indicado quando o consumidor precisa do arquivo completo em memória. Para arquivos muito
    /// grandes prefira <see cref="LerStreamingAsync"/>.
    /// </summary>
    public Task<ArquivoEfdContribuicoes> LerAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEfdContribuicoes.CarregarAsync(LerStreamingAsync(entrada, cancelamento), cancelamento);
}
