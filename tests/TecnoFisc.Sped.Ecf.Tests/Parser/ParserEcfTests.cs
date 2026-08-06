using TecnoFisc.Sped.Ecf.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ParserEcfTests
{
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
