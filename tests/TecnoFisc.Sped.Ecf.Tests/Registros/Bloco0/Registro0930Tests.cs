using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0930Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0930(), "0930", "1:N");
    }

    [Theory]
    [InlineData("12345678909")]
    [InlineData("11111111000191")]
    public void Parser_PreservaIdentificadorCompostoCpfCnpj(string identificador)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|0930|SIGNATARIO SINTETICO|{identificador}|900|CRC-SINTETICO|assinante@exemplo.br|06133334444|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro0930>()
            .Which.IdentCpfCnpj.Should().Be(identificador);
    }
}
