using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0030(), "0030", "1:1");
    }

    [Fact]
    public void Parser_PreservaZerosSignificativosDosCodigosEDoTelefone()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|0030|0204|0620400|RUA TESTE|123|SALA 1|BAIRRO|DF|5300108|71000000|06133333333|teste@exemplo.br|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro0030>().Which;
        registro.CodNat.Should().Be("0204");
        registro.CnaeFiscal.Should().Be("0620400");
        registro.CodMun.Should().Be("5300108");
        registro.Cep.Should().Be("71000000");
        registro.NumTel.Should().Be("06133333333");
    }
}
