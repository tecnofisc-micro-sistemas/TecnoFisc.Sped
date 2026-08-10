using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoV;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoV;

public sealed class RegistroV020Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroV020(), "V020", "1:N");
    }

    [Theory]
    [InlineData("CPF", "12345678900")]
    [InlineData("CNPJ", "12345678000195")]
    [InlineData("PS", "PASS-12345")]
    public void Parser_PreservaTipoENumeroDoDocumentoGenerico(string tipo, string numero)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|V020|RESPONSAVEL|RUA ALFA 121|{tipo}|{numero}|12345-6|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroV020>().Which;
        registro.Nome.Should().Be("RESPONSAVEL");
        registro.Endereco.Should().Be("RUA ALFA 121");
        registro.TipoDoC.Should().Be(tipo);
        registro.Ni.Should().Be(numero);
        registro.IdentConta.Should().Be("12345-6");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
