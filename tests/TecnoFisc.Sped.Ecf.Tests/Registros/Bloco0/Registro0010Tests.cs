using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0010Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0010(), "0010", "1:1");
    }

    [Fact]
    public void Parser_PreservaCodigosFiscaisELeIndicadorSimNao()
    {
        var resultado = new ParserEcf().ParseLinha("|0010||N|1|T|01|RRRR||||||1|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro0010>().Which;
        registro.OptRefis.Should().Be(IndicadorSimNao.Nao);
        registro.FormaTrib.Should().Be("1");
        registro.FormaApur.Should().Be("T");
        registro.CodQualifPj.Should().Be("01");
        registro.FormaTribPer.Should().Be("RRRR");
        registro.IndRecReceita.Should().Be("1");
    }
}
