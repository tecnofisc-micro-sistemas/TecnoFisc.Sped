using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX480Tests
{
    [Fact]
    public void Registro_ConformeManifestoComValorNumericoAssinado()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX480(), "X480", "0:N");
    }

    [Theory]
    [InlineData("-1234,56", -1234.56)]
    [InlineData("0,01", 0.01)]
    public void Parser_LeEscalaESinalDoValor(string bruto, double esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|X480|000001|BENEFICIO|{bruto}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX480>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Valor.Should().Be((decimal)esperado);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ValorVazioOuInvalido_PreservaNuloOuRegistraErro()
    {
        var vazio = new ParserEcf().ParseLinha("|X480|000001|||");
        var invalido = new ParserEcf().ParseLinha("|X480|000001||INVALIDO|");

        vazio.Valor.Should().BeOfType<RegistroX480>().Which.Valor.Should().BeNull();
        invalido.Valor.Should().BeOfType<RegistroX480>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX480.Valor));
    }
}
