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
}
