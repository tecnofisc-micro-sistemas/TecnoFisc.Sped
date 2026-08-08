using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE010Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE010(), "E010", "0:N");
    }

    [Fact]
    public void Parser_LeNaturezaContaValorEIndicador()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E010|0101|REF001|CONTA REFERENCIAL|1000,50|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE010>().Which;
        registro.CodNat.Should().Be("0101");
        registro.CodCtaRef.Should().Be("REF001");
        registro.DescCtaRef.Should().Be("CONTA REFERENCIAL");
        registro.ValCtaRef.Should().Be(1000.50m);
        registro.IndValCtaRef.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Fact]
    public void Parser_ValorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E010|0101|REF001|CONTA REFERENCIAL|INVALIDO|D|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE010>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro => erro.Campo == "VAL_CTA_REF");
    }
}
