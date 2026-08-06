using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN001Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN001(), "N001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|N001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN001>()
            .Which.IndDad.Should().Be(esperado);
    }

    [Fact]
    public void Parser_IndicadorForaDoDominio_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N001|2|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN001>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroN001.IndDad) && erro.ValorBruto == "2");
    }
}
