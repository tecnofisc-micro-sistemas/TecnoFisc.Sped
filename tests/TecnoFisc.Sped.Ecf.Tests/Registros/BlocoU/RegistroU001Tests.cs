using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU001Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU001(), "U001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|U001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU001>()
            .Which.IndDad.Should().Be(esperado);
    }

    [Fact]
    public void Parser_IndicadorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|U001|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU001>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroU001.IndDad) && erro.ValorBruto == "X");
    }
}
