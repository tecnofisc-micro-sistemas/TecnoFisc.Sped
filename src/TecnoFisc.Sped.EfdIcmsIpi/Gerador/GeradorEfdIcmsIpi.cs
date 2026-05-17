using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;

namespace TecnoFisc.Sped.EfdIcmsIpi.Gerador;

/// <summary>
/// Escritor especializado do leiaute EFD ICMS-IPI (baseline V015). Resolve o catálogo
/// dos registros declarados em <see cref="TecnoFisc.Sped.EfdIcmsIpi"/> e delega a escrita
/// ao <see cref="EscritorSpedTxt"/> compartilhado pelo Core.
/// </summary>
/// <remarks>
/// O gerador grava exatamente o que vier no enumerador — registros de encerramento e
/// totalizadores são responsabilidade do montador.
/// </remarks>
public sealed class GeradorEfdIcmsIpi : IEscritorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao =
        CatalogoBuilder.BuildFromAssembly(typeof(GeradorEfdIcmsIpi).Assembly);

    private readonly EscritorSpedTxt _escritor;

    /// <summary>Cria o gerador usando o catálogo padrão do assembly de EFD ICMS-IPI.</summary>
    public GeradorEfdIcmsIpi() : this(_catalogoPadrao)
    {
    }

    /// <summary>Cria o gerador com um catálogo customizado (testes, source generator).</summary>
    public GeradorEfdIcmsIpi(IRegistroSpedCatalogo catalogo)
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
    /// a coleção em memória.
    /// </summary>
    public Task EscreverAsync(
        Stream saida,
        IEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
        => _escritor.EscreverAsync(saida, registros, cancelamento);
}
