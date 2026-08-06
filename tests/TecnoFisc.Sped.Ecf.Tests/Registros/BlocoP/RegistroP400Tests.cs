using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP400Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP400(), "P400", "0:N");
    }

    [Fact]
    public void Parser_PreservaValorTextualSemAplicarRegraDaBaseDeCalculo()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P400|0018|(-)EXCEDENTE DE VARIACAO CAMBIAL|00010000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP400>().Which;
        registro.CampoCodigo.Should().Be("0018");
        registro.Descricao.Should().Be("(-)EXCEDENTE DE VARIACAO CAMBIAL");
        registro.Valor.Should().Be("00010000,00");
    }
}
