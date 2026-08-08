using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Versionamento;
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
        _leitor = new LeitorSpedTxt(catalogo, ResolveOptions(opcoes));
    }

    /// <summary>
    /// Resolve as opções do chamador contra os padrões do leiaute ECF: vigência e validação
    /// de domínio ligadas quando o chamador não se pronunciou; override explícito sempre vence.
    /// </summary>
    internal static ReadingOptions ResolveOptions(ReadingOptions opcoes)
        => new()
        {
            RegistrosIgnorados = opcoes.RegistrosIgnorados,
            BlocosIgnorados = opcoes.BlocosIgnorados,
            LenientFieldParsing = opcoes.LenientFieldParsing,
            LenientLayout = opcoes.LenientLayout,
            RespeitarVigenciaDoLeiaute = opcoes.RespeitarVigenciaDoLeiaute ?? true,
            ValidarDominioDeEnum = opcoes.ValidarDominioDeEnum ?? true,
        };

    /// <summary>Lê um registro por vez, preservando os vínculos hierárquicos.</summary>
    public IAsyncEnumerable<RegistroSped> ReadStreamingAsync(
        Stream entrada,
        CancellationToken cancelamento = default)
        => _leitor.ReadStreamingAsync(entrada, cancelamento);

    /// <summary>Lê o fluxo inteiro e agrupa os registros no arquivo ECF.</summary>
    public Task<ArquivoEcf> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
        => ArquivoEcf.LoadAsync(ReadStreamingAsync(entrada, cancelamento), cancelamento);

    /// <summary>
    /// Interpreta uma única linha ECF sem construir a hierarquia e sem aplicar vigência — todos
    /// os campos do catálogo são aceitos, inclusive os introduzidos em leiautes posteriores. Use
    /// a sobrecarga com <see cref="LayoutEcf"/> quando a vigência do leiaute importar.
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha);

    /// <summary>
    /// Interpreta uma única linha ECF sem construir a hierarquia, aplicando a vigência do
    /// <paramref name="leiaute"/> informado — o mesmo critério que <c>ReadStreamingAsync</c>
    /// aplica a partir do <c>COD_VER</c> do arquivo. A sobrecarga sem <paramref name="leiaute"/>
    /// não aplica vigência nenhuma.
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(
        ReadOnlySpan<char> linha, LayoutEcf leiaute, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha, (int)leiaute);
}
