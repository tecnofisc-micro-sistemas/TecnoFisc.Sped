using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;

namespace TecnoFisc.Sped.EfdContribuicoes.Gerador;

/// <summary>
/// Escritor especializado do leiaute EFD Contribuições (V006, guia v1.35). Resolve o catálogo
/// dos registros declarados em <see cref="TecnoFisc.Sped.EfdContribuicoes"/> e delega a escrita
/// ao <see cref="EscritorSpedTxt"/> compartilhado pelo Core.
/// </summary>
/// <remarks>
/// O gerador grava exatamente o que vier no enumerador — registros de encerramento (X990, 9999)
/// e o totalizador 9900 são responsabilidade do montador (<c>TotalizadorBlocos</c> do Stage 3).
/// </remarks>
public sealed class GeradorEfdContribuicoes : IEscritorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao =
        CatalogoBuilder.BuildFromAssembly(typeof(GeradorEfdContribuicoes).Assembly);

    private readonly EscritorSpedTxt _escritor;

    /// <summary>Cria o gerador usando o catálogo padrão do assembly de EFD Contribuições.</summary>
    public GeradorEfdContribuicoes() : this(_catalogoPadrao)
    {
    }

    /// <summary>Cria o gerador com um catálogo customizado (testes, source generator).</summary>
    public GeradorEfdContribuicoes(IRegistroSpedCatalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        _escritor = new EscritorSpedTxt(catalogo);
    }

    /// <inheritdoc />
    public Task EscreverAsync(
        Stream saida,
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
        => _escritor.EscreverAsync(saida, registros, cancelamento);

    /// <summary>
    /// Sobrecarga sobre <see cref="IEnumerable{RegistroSped}"/> para consumidores que já têm
    /// a coleção em memória (vinda, por exemplo, de <c>ArquivoEfdContribuicoes</c>).
    /// </summary>
    public Task EscreverAsync(
        Stream saida,
        IEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
        => _escritor.EscreverAsync(saida, registros, cancelamento);
}
