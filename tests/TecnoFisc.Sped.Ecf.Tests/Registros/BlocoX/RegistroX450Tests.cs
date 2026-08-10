using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX450Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX450(), "X450", "0:N");
    }

    [Fact]
    public void Parser_PreservaPaisNumericoComoCodigoLexical()
    {
        var resultado = new ParserEcf().ParseLinha("|X450|005|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX450>().Which;
        registro.Pais.Should().Be("005");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
