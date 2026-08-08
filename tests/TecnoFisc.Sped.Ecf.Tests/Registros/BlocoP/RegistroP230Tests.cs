using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP230Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP230(), "P230", "0:N");
    }

    [Fact]
    public void Parser_PreservaPercentualComoDadoTextualSemCalculo()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P230|0037|TOTAL DA ISENCAO E REDUCAO|12,3400%|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP230>().Which;
        registro.CampoCodigo.Should().Be("0037");
        registro.Descricao.Should().Be("TOTAL DA ISENCAO E REDUCAO");
        registro.Valor.Should().Be("12,3400%");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
