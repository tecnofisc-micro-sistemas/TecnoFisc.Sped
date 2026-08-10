using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX460Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX460(), "X460", "0:N");
    }

    [Fact]
    public void Parser_PreservaCamposDinamicosLongosEOptionais()
    {
        const string valor = "MEMORIA DE CALCULO: 00001234 / -9.876,54";
        var resultado = new ParserEcf().ParseLinha($"|X460|000001|INOVACAO TECNOLOGICA|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX460>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().Be("INOVACAO TECNOLOGICA");
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
