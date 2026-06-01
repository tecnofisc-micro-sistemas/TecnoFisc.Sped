using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Gerador;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests;

/// <summary>
/// Modelo raiz do arquivo EFD Contribuições. Cobre a classificação de registros por bloco,
/// a ordem canônica de enumeração (0 → A → C → D → F → I → M → P → 1 → 9) e o round-trip
/// completo Parser → Arquivo → Gerador.
/// </summary>
public sealed class ArquivoEfdContribuicoesTests
{
    [Fact]
    public void EnumerarBlocos_ExpoeOsDezBlocosNaOrdemCanonica()
    {
        var arquivo = new ArquivoEfdContribuicoes();

        arquivo.EnumerarBlocos().Select(b => b.Identificador)
            .Should().Equal(["0", "A", "C", "D", "F", "I", "M", "P", "1", "9"]);
    }

    [Fact]
    public void Adicionar_ClassificaRegistroNoBlocoCorrespondente()
    {
        var arquivo = new ArquivoEfdContribuicoes();
        var r0001 = new Registro0001 { IndMov = IndicadorMovimentoBloco.ComDados };
        var r9001 = new Registro9001 { IndMov = IndicadorMovimentoBloco.ComDados };

        arquivo.Adicionar(r0001);
        arquivo.Adicionar(r9001);

        arquivo.Bloco0.Registros.Should().Equal([r0001]);
        arquivo.Bloco9.Registros.Should().Equal([r9001]);
        arquivo.BlocoA.Registros.Should().BeEmpty();
    }

    [Fact]
    public void EnumerarRegistros_RetornaTodosOsBlocosNaOrdemCanonica()
    {
        var arquivo = new ArquivoEfdContribuicoes();
        var r9001 = new Registro9001 { IndMov = IndicadorMovimentoBloco.ComDados };
        var r0001 = new Registro0001 { IndMov = IndicadorMovimentoBloco.ComDados };
        var r9999 = new Registro9999 { QtdLin = 3 };
        var r0990 = new Registro0990 { QtdLin0 = 2 };

        // Adiciona fora de ordem para provar que o arquivo reordena por bloco.
        arquivo.Adicionar(r9001);
        arquivo.Adicionar(r0001);
        arquivo.Adicionar(r9999);
        arquivo.Adicionar(r0990);

        arquivo.EnumerarRegistros().Should().Equal([r0001, r0990, r9001, r9999]);
    }

    [Fact]
    public void Adicionar_PreservaOrdemDentroDoMesmoBloco()
    {
        var arquivo = new ArquivoEfdContribuicoes();
        var primeiro = new Registro9900 { RegBlc = "0000", QtdRegBlc = 1 };
        var segundo = new Registro9900 { RegBlc = "9999", QtdRegBlc = 1 };

        arquivo.Adicionar(primeiro);
        arquivo.Adicionar(segundo);

        arquivo.Bloco9.Registros.Should().Equal([primeiro, segundo]);
    }

    [Fact]
    public void Adicionar_QuandoRegistroNulo_LancaArgumentNullException()
    {
        var arquivo = new ArquivoEfdContribuicoes();

        var act = () => arquivo.Adicionar(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CarregarAsync_QuandoFluxoNulo_LancaArgumentNullException()
    {
        var act = async () => await ArquivoEfdContribuicoes.LoadAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RoundTrip_ParserArquivoGerador_PreservaTextoCanonico()
    {
        const string sped =
            "|0001|0|\r\n" +
            "|0990|2|\r\n" +
            "|9001|0|\r\n" +
            "|9900|0000|1|\r\n" +
            "|9900|9999|1|\r\n" +
            "|9990|3|\r\n" +
            "|9999|7|\r\n";

        var parser = new ParserEfdContribuicoes();
        var gerador = new GeradorEfdContribuicoes();

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var arquivo = await ArquivoEfdContribuicoes.LoadAsync(
            parser.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);

        using var saida = new MemoryStream();
        await gerador.WriteAsync(saida, arquivo.EnumerarRegistros(), TestContext.Current.CancellationToken);

        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(sped);
    }
}
