using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoV;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoV;

public sealed class RegistroV030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroV030(), "V030", "1:12");
    }

    [Theory]
    [InlineData("01")]
    [InlineData("12")]
    [InlineData("13")]
    public void Parser_PreservaMesSemExecutarRegraDependenteDoPeriodo(string mes)
    {
        var resultado = new ParserEcf().ParseLinha($"|V030|{mes}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroV030>().Which;
        registro.Mes.Should().Be(mes);
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
