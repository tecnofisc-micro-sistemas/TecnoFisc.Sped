using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco9;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco9;

public sealed class Registro9990Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistro9990()
    {
        AssertRegistroEcf.CodesAreImplemented("9990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro9990(), "9990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhasDoBloco9()
    {
        var resultado = new ParserEcf().ParseLinha("|9990|9|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro9990>().Which.QtdLin.Should().Be(9);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|9990|INVALIDA|").Valor
            .Should().BeOfType<Registro9990>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == nameof(Registro9990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }
}
