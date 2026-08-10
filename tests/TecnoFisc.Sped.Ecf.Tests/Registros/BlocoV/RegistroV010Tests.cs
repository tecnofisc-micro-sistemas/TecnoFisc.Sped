using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoV;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoV;

public sealed class RegistroV010Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroV010(), "V010", "1:N");
    }

    [Fact]
    public void Parser_PreservaInstituicaoPaisEMoedaDeTabelasDinamicas()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|V010|BANCO INTERNACIONAL|BR|EUR|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroV010>().Which;
        registro.NomeInstituicao.Should().Be("BANCO INTERNACIONAL");
        registro.Pais.Should().Be("BR");
        registro.TipMoeda.Should().Be("EUR");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
