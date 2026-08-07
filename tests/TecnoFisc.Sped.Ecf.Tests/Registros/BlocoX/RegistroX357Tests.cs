using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX357Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX357(), "X357", "0:N");
    }

    [Theory]
    [InlineData("005", "0000")]
    [InlineData("249", "00123456000199")]
    [InlineData("840", "NIF-LONGO-00A9Z9")]
    public void Parser_PreservaPaisENifOuCnpjComoIdentificadoresLexicais(string pais, string nifCnpj)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|X357|{pais}|{nifCnpj}|INVESTIDORA DIRETA|25,5000|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX357>().Which;
        registro.Pais.Should().Be(pais);
        registro.NifCnpj.Should().Be(nifCnpj);
        registro.RazaoSocial.Should().Be("INVESTIDORA DIRETA");
        registro.Percentual.Should().Be(25.5000m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_PercentualInvalido_RegistraErroSemCoagirIdentificadorComposto()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X357|005|NIF-A9|INVESTIDORA DIRETA|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX357>().Which;
        registro.NifCnpj.Should().Be("NIF-A9");
        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "PERCENTUAL" && erro.ValorBruto == "INVALIDO");
    }
}
