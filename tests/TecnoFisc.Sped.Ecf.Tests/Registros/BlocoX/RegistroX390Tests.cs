using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX390Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX390(), "X390", "0:N");
    }

    [Fact]
    public void Parser_PreservaOrigemEAplicacaoDinamicasSemCoagirValor()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X390|CODIGO-ALFANUMERICO|APLICACAO DE RECURSOS|R$ 9.876,54 C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX390>().Which;
        registro.CampoCodigo.Should().Be("CODIGO-ALFANUMERICO");
        registro.Descricao.Should().Be("APLICACAO DE RECURSOS");
        registro.Valor.Should().Be("R$ 9.876,54 C");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
