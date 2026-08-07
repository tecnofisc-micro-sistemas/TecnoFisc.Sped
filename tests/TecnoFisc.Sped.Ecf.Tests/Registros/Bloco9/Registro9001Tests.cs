using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco9;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco9;

public sealed class Registro9001Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistro9001()
    {
        AssertRegistroEcf.CodesAreImplemented("9001");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro9001(), "9001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeDominioCompletoDoIndicadorDeMovimento(
        string token,
        IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|9001|{token}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro9001>().Which.IndDad.Should().Be(esperado);
    }

    [Fact]
    public void Parser_IndicadorDeMovimentoInvalido_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|9001|2|").Valor
            .Should().BeOfType<Registro9001>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "IND_DAD" && erro.ValorBruto == "2");
    }
}
