using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

/// <summary>
/// Prova que a coluna barrada por vigência é sempre consumida pela posição, mesmo quando está
/// fisicamente presente no arquivo abaixo da versão de introdução do campo (achado 4 do PR 531):
/// o cursor sequencial antigo assumia que a coluna estaria ausente e deslocava o restante da
/// linha um campo à esquerda quando ela existia. <see cref="RegistroVigenciaColunaSintetico"/>
/// (A300) encadeia dois campos versionados com limiares crescentes (12, depois 20) — a única
/// forma que <c>CatalogoBuilder.ValidarVigenciaCrescente</c> aceita hoje; um campo sempre
/// presente ou de versão anterior depois de um campo versionado é rejeitado na construção do
/// catálogo (ver <c>RegistroSpedCatalogoGeneratorVigenciaTests</c>), não apenas evitado aqui em
/// tempo de leitura. Exercita o caminho real (<see cref="LeitorSpedTxt.ReadStreamingAsync"/>) com
/// um <c>0000</c> sintético que declara a versão, em vez de acessar <c>InterpretarLinha</c>
/// diretamente — não amplia a superfície interna/pública da biblioteca.
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
