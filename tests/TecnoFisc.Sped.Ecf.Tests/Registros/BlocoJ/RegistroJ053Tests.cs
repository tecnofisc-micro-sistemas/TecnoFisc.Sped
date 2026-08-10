using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoJ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoJ;

public sealed class RegistroJ053Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroJ053(), "J053", "0:N");
    }

    [Theory]
    [InlineData("02")]
    [InlineData("99")]
    public void Parser_PreservaGrupoSubcontaENaturezaComoCodigosLossless(string natureza)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|J053|000123|0001.0002|{natureza}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroJ053>().Which;
        registro.CodIdt.Should().Be("000123");
        registro.CodCntCorr.Should().Be("0001.0002");
        registro.NatSubCnt.Should().Be(natureza);
    }
}
