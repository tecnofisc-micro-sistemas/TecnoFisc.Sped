using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ParserEcfTests
{
    [Fact]
    public async Task ReadAsync_Minimo_MaterializaBlocos0E9()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "minimo.txt");
        File.Exists(caminho).Should().BeTrue($"a fixture deveria ter sido copiada para '{caminho}'");

        await using var entrada = File.OpenRead(caminho);
        var arquivo = await new ParserEcf().ReadAsync(entrada, TestContext.Current.CancellationToken);

        arquivo.EnumerarBlocos().Select(bloco => bloco.Identificador)
            .Should().Equal([
                "0", "C", "E", "J", "K", "L", "M", "N", "P",
                "Q", "T", "U", "V", "W", "X", "Y", "9",
            ]);
        var abertura = arquivo.Bloco0.Registros.Should().ContainSingle().Which;
        abertura.Should().BeOfType<Registro0000>();
        AssertRegistroEcf.ConformsToManifest(abertura, "0000", "1:1");
        arquivo.Bloco9.Registros.Should().BeEmpty(
            "o registro 9999 só será implementado na Task 31; o bloco 9 já deve existir estruturalmente");
    }

    [Fact]
    public void ParseLinha_SemRegistrosCatalogados_RetornaFalhaDeLayout()
    {
        var parser = new ParserEcf();

        var resultado = parser.ParseLinha("|Z001|CONTEUDO|");

        resultado.Falha.Should().BeTrue();
        resultado.Erros.Should().ContainSingle();
        resultado.Erros[0].CodigoRegistro.Should().Be("Z001");
    }

    [Fact]
    public void ParseLinha_CatalogoPadraoResolveRegistro0000()
    {
        var parser = new ParserEcf();

        var resultado = parser.ParseLinha(
            "|0000|LECF|0011|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<global::TecnoFisc.Sped.Ecf.Registros.Bloco0.Registro0000>();
    }
}
