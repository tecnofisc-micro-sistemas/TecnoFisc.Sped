using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC053Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC053(), "C053", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigosComZerosSignificativos()
    {
        var resultado = new ParserEcf().ParseLinha("|C053|000123|000045|02|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC053>().Which;
        registro.CodIdt.Should().Be("000123");
        registro.CodCntCorr.Should().Be("000045");
        registro.NatSubCnt.Should().Be("02");
    }
}
