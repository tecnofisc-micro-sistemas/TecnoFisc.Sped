using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM010Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM010(), "M010", "0:N");
    }

    [Theory]
    [InlineData(nameof(RegistroM010.DtApLal), typeof(DateOnly), true)]
    [InlineData(nameof(RegistroM010.DtLimLal), typeof(DateOnly?), false)]
    public void DatasLexicaisC8_UsamDateOnlyEFormatoExato(
        string nomePropriedade,
        Type tipoEsperado,
        bool obrigatorio)
    {
        PropertyInfo propriedade = typeof(RegistroM010).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        propriedade.PropertyType.Should().Be(tipoEsperado);
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
        campo.Obrigatorio.Should().Be(obrigatorio);
    }

    [Fact]
    public void Parser_LeDatasCnpjSaldoDominiosECodigosSemPerda()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M010|000123|CONTA DA PARTE B|01012018|001001|31122026|I|1000,25|D|11222333000181|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM010>().Which;
        registro.CodCtaB.Should().Be("000123");
        registro.DtApLal.Should().Be(new DateOnly(2018, 1, 1));
        registro.CodPbRfb.Should().Be("001001");
        registro.DtLimLal.Should().Be(new DateOnly(2026, 12, 31));
        registro.CodTributo.Should().Be(IndicadorTributoContaParteB.Irpj);
        registro.VlSaldoIni.Should().Be(1000.25m);
        registro.IndVlSaldoIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.CnpjSitEsp.Should().Be(Cnpj.Create("11222333000181"));
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M010|000123|CONTA DA PARTE B|01012018|001001||C|0,00|C||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM010>().Which;
        registro.DtLimLal.Should().BeNull();
        registro.CnpjSitEsp.Should().BeNull();
    }

    [Fact]
    public void Parser_DatasDominiosECnpjInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M010|000123|CONTA|20180101|001001|20261231|A|INVALIDO|X|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM010>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroM010.DtApLal),
                nameof(RegistroM010.DtLimLal),
                nameof(RegistroM010.CodTributo),
                nameof(RegistroM010.VlSaldoIni),
                nameof(RegistroM010.IndVlSaldoIni),
                nameof(RegistroM010.CnpjSitEsp),
            ]);
    }
}
