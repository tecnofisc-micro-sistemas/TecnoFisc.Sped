using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Parser;

/// <summary>Leitor especializado dos leiautes ECF suportados.</summary>
public sealed class ParserEcf : ILeitorSped
{
    private static readonly IRegistroSpedCatalogo _catalogoPadrao = new CatalogoSpedGerado();

    private readonly LeitorSpedTxt _leitor;

    /// <summary>Cria o parser com o catálogo gerado em tempo de compilação.</summary>
    public ParserEcf() : this(_catalogoPadrao, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com opções de leitura e o catálogo gerado.</summary>
    public ParserEcf(ReadingOptions opcoes) : this(_catalogoPadrao, opcoes)
    {
    }

    /// <summary>Cria o parser com um catálogo customizado.</summary>
    public ParserEcf(IRegistroSpedCatalogo catalogo) : this(catalogo, ReadingOptions.Default)
    {
    }

    /// <summary>Cria o parser com catálogo e opções de leitura customizados.</summary>
    public ParserEcf(IRegistroSpedCatalogo catalogo, ReadingOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(opcoes);
        _leitor = new LeitorSpedTxt(catalogo, ComVigenciaDoLeiaute(opcoes));
    }

    private static ReadingOptions ComVigenciaDoLeiaute(ReadingOptions opcoes)
        => new()
        {
            RegistrosIgnorados = opcoes.RegistrosIgnorados,
            BlocosIgnorados = opcoes.BlocosIgnorados,
            LenientFieldParsing = opcoes.LenientFieldParsing,
            LenientLayout = opcoes.LenientLayout,
            RespeitarVigenciaDoLeiaute = true,
        };

    /// <summary>Lê um registro por vez, preservando os vínculos hierárquicos.</summary>
    public IAsyncEnumerable<RegistroSped> ReadStreamingAsync(
        Stream entrada,
        CancellationToken cancelamento = default)
        => _leitor.ReadStreamingAsync(entrada, cancelamento);

    /// <summary>Lê o fluxo inteiro e agrupa os registros no arquivo ECF.</summary>
    public Task<ArquivoEcf> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEcf.LoadAsync(ReadStreamingAsync(entrada, cancelamento), cancelamento);

    /// <summary>Interpreta uma única linha ECF sem construir a hierarquia.</summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha);

}
