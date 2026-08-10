using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0021Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0021(), "0021", "0:1");
    }

    [Fact]
    public void Parser_LeIndicadoresOpcionais()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|0021|S|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro0021>().Which;
        registro.IndRepes.Should().Be(IndicadorSimNao.Sim);
        registro.IndRecap.Should().Be(IndicadorSimNao.Nao);
        registro.IndRepetroTemporario.Should().Be(IndicadorSimNao.Nao);
    }
}
