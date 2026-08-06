using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC050Tests
{
    private static readonly IRegistroSpedCatalogo _catalogoGerado = new CatalogoSpedGerado();
    private static readonly IRegistroSpedCatalogo _catalogoReflexivo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC050).Assembly);

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC050(), "C050", "0:N");
    }

    [Fact]
    public void Parser_LeDataNaturezaTipoNivelEPreservaCodigos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|C050|01012025|01|A|4|001.01|001|CAIXA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC050>().Which;
        registro.DtAlt.Should().Be(new DateOnly(2025, 1, 1));
        registro.CodNat.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        registro.IndCta.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nível.Should().Be(4);
        registro.CodCta.Should().Be("001.01");
        registro.CodCtaSup.Should().Be("001");
    }

    [Fact]
    public void Parser_CodigoNaturezaIndefinido_RegistraErroSemAtribuirValor()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|C050|01012025|06|A|4|001.01|001|CAIXA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC050>().Which;
        registro.CodNat.Should().Be(default);
        registro.ErrosDeFormato.Should().ContainSingle()
            .Which.Campo.Should().Be(nameof(RegistroC050.CodNat));
    }

    [Theory]
    [InlineData(nameof(CodigoNaturezaContaContabil.ContasDeAtivo), 0, true)]
    [InlineData("contasDeAtivo", 0, true)]
    [InlineData("01", 1, false)]
    [InlineData("06", 0, true)]
    [InlineData("", 0, false)]
    [InlineData("2147483648", 0, true)]
    [InlineData("+01", 1, false)]
    [InlineData("-1", 0, true)]
    [InlineData(" 0001 ", 1, false)]
    public void Parser_CatalogosGeradoEReflexivo_MantemParidadeParaEnumNumerico(
        string valorBruto,
        int valorEsperado,
        bool esperaErro)
    {
        var gerado = ParsearCom(_catalogoGerado, valorBruto);
        var reflexivo = ParsearCom(_catalogoReflexivo, valorBruto);

        ((int)gerado.CodNat).Should().Be(valorEsperado);
        gerado.ErrosDeFormato.Should().HaveCount(esperaErro ? 1 : 0);
        ((int)reflexivo.CodNat).Should().Be(valorEsperado);
        reflexivo.ErrosDeFormato
            .Select(erro => (erro.Campo, erro.ValorBruto))
            .Should().Equal(gerado.ErrosDeFormato.Select(erro => (erro.Campo, erro.ValorBruto)));
    }

    private static RegistroC050 ParsearCom(IRegistroSpedCatalogo catalogo, string valorBruto)
    {
        var resultado = new ParserEcf(catalogo).ParseLinha(
            $"|C050|01012025|{valorBruto}|A|4|001.01|001|CAIXA|");

        resultado.Sucesso.Should().BeTrue();
        return resultado.Valor.Should().BeOfType<RegistroC050>().Which;
    }
}
