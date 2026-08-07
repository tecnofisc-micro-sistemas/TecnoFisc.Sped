using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE020Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE020(), "E020", "0:N");
    }

    [Theory]
    [InlineData(nameof(RegistroE020.DtApLal))]
    [InlineData(nameof(RegistroE020.DtLimLal))]
    public void DatasLexicaisTipoC_TemDateOnlyNullableEFormatoExato(string nomePropriedade)
    {
        PropertyInfo propriedade = typeof(RegistroE020).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        propriedade.PropertyType.Should().Be<DateOnly?>();
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
    }

    [Fact]
    public void Parser_LeDatasTributoSaldoIndicadorECodigosLossless()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E020|000123|PREJUIZO FISCAL|31122024|31122029|A|250,75|D|000045|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE020>().Which;
        registro.CodCtaB.Should().Be("000123");
        registro.DtApLal.Should().Be(new DateOnly(2024, 12, 31));
        registro.DtLimLal.Should().Be(new DateOnly(2029, 12, 31));
        registro.Tributo.Should().Be(IndicadorTributoParteB.Ambos);
        registro.VlSaldoFin.Should().Be(250.75m);
        registro.IndVlSaldoFin.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.CodPbRfb.Should().Be("000045");
    }

    [Theory]
    [InlineData("I", IndicadorTributoParteB.Irpj)]
    [InlineData("C", IndicadorTributoParteB.Csll)]
    [InlineData("A", IndicadorTributoParteB.Ambos)]
    public void Parser_LeDominioFechadoDeTributo(
        string valor,
        IndicadorTributoParteB esperado)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|E020|000123||||{valor}|||000045|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE020>()
            .Which.Tributo.Should().Be(esperado);
    }

    [Fact]
    public void Parser_CamposOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|E020|000999||||||000001|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE020>().Which;
        registro.DescCtaLal.Should().BeNull();
        registro.DtApLal.Should().BeNull();
        registro.DtLimLal.Should().BeNull();
        registro.Tributo.Should().BeNull();
        registro.VlSaldoFin.Should().BeNull();
        registro.IndVlSaldoFin.Should().BeNull();
    }

    [Fact]
    public void Parser_DataNoFormatoIso_RejeitaAmbosCamposDeData()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E020|000123||20241231|20291231|I|1,00|C|000045|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE020>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["DT_AP_LAL", "DT_LIM_LAL"]);
    }
}
