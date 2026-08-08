using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

/// <summary>
/// Prova que um campo fora de vigência (<c>DesdeVersao</c> posterior à versão declarada no
/// <c>0000</c>) simplesmente não é atribuído — a função local <c>CampoAtivo</c> de
/// <c>LeitorSpedTxt.InterpretarLinha</c> combinada com o mapeamento posicional
/// (<c>indice = posicaoCampo - 2</c>) — sem afetar os campos vizinhos.
/// <see cref="RegistroVigenciaColunaSintetico"/> (A300) encadeia dois campos versionados com
/// limiares crescentes (12, depois 20).
/// <para>
/// Não prova o deslocamento do cursor sequencial antigo (achado 4 do PR 531): com a invariante de
/// vigência não-decrescente agora imposta na construção do catálogo (<c>DesdeVersao</c> não pode
/// decrescer ao longo da posição — ver <c>CatalogoBuilder.ValidarVigenciaCrescente</c> e o
/// diagnóstico <c>TFSPED003</c> do source generator), cursor sequencial e índice posicional passam
/// a ser equivalentes para qualquer catálogo válido, e esta suíte não os distingue mais. O bug
/// original — cursor deslocando as colunas seguintes quando um campo barrado estava fisicamente
/// presente no arquivo — é o que <c>RegistroSpedCatalogoGeneratorVigenciaTests</c> prova ao rejeitar
/// a construção de um catálogo com vigência fora de ordem.
/// </para>
/// Exercita o caminho real (<see cref="LeitorSpedTxt.ReadStreamingAsync"/>) com um <c>0000</c>
/// sintético que declara a versão, em vez de acessar <c>InterpretarLinha</c> diretamente — não
/// amplia a superfície interna/pública da biblioteca.
/// </summary>
public sealed class VigenciaCampoPosicionalTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroVigenciaColunaSintetico).Assembly);

    private static async Task<RegistroVigenciaColunaSintetico> ReadAsync(int versao)
    {
        string sped =
            $"|0000|{versao:D3}|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|A300|a|n|d|\r\n" +
            "|9999|2|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo, new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));

        RegistroVigenciaColunaSintetico? registro = null;
        await foreach (var r in leitor.ReadStreamingAsync(stream))
        {
            if (r is RegistroVigenciaColunaSintetico alvo)
                registro = alvo;
        }

        return registro ?? throw new InvalidOperationException("A300 não foi lido.");
    }

    [Fact]
    public async Task ColunasBarradasPresentesNoArquivo_NaoSaoAtribuidas()
    {
        var registro = await ReadAsync(versao: 10);

        registro.Antes.Should().Be("a");
        registro.Novo.Should().BeNull();
        registro.Depois.Should().BeNull();
    }

    [Fact]
    public async Task VersaoNoLimite_AtribuiOCampoNovoSemAfetarOAindaBarrado()
    {
        var registro = await ReadAsync(versao: 12);

        registro.Antes.Should().Be("a");
        registro.Novo.Should().Be("n");
        registro.Depois.Should().BeNull();
    }

    [Fact]
    public async Task VersaoAlcancaOSegundoLimite_AtribuiTodosOsCamposAtivos()
    {
        var registro = await ReadAsync(versao: 20);

        registro.Antes.Should().Be("a");
        registro.Novo.Should().Be("n");
        registro.Depois.Should().Be("d");
    }
}
