using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Generated;

namespace TecnoFisc.Sped.EfdIcmsIpi.Parser;

/// <summary>
/// Leitor especializado do leiaute EFD ICMS-IPI (baseline V306). Resolve o catálogo
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
    public ParserEfdIcmsIpi() : this(_catalogoPadrao)
    {
    }

    /// <summary>Cria o parser com um catálogo customizado (testes, source generator).</summary>
    public ParserEfdIcmsIpi(IRegistroSpedCatalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        _leitor = new LeitorSpedTxt(catalogo);
    }

    /// <summary>
    /// Lê o fluxo SPED em modo streaming, materializando um registro por vez sem bufferizar o
    /// arquivo todo. Os registros saem com Pai/Filhos já vinculados.
    /// </summary>
    public IAsyncEnumerable<RegistroSped> LerStreamingAsync(Stream entrada, CancellationToken cancelamento = default)
        => _leitor.LerStreamingAsync(entrada, cancelamento);

    /// <summary>
    /// Lê o fluxo SPED inteiro e devolve o modelo tipado <see cref="ArquivoEfdIcmsIpi"/> com
    /// todos os blocos populados. Para arquivos muito grandes prefira <see cref="LerStreamingAsync"/>.
    /// </summary>
    public Task<ArquivoEfdIcmsIpi> LerAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEfdIcmsIpi.CarregarAsync(LerStreamingAsync(entrada, cancelamento), cancelamento);
}
