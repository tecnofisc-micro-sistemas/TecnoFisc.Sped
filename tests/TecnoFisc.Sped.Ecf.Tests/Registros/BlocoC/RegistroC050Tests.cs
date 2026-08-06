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
            .Which.Should().BeEquivalentTo(new
            {
                Linha = 0L,
                CodigoRegistro = "C050",
                Campo = nameof(RegistroC050.CodNat),
                ValorBruto = "06",
                Mensagem = "Valor '06' não é válido para CodigoNaturezaContaContabil.",
            });
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
        reflexivo.ErrosDeFormato.Should().Equal(gerado.ErrosDeFormato);
    }

    [Theory]
    [InlineData("X", "Valor 'X' não é válido para IndicadorTipoConta.")]
    [InlineData(nameof(IndicadorTipoConta.Analitica),
        "Valor 'Analitica' não é válido para IndicadorTipoConta.")]
    public void Parser_CatalogosGeradoEReflexivo_MantemErroCompletoParaSpedValor(
        string valorBruto,
        string mensagemEsperada)
    {
        var gerado = ParsearCom(_catalogoGerado, "01", valorBruto);
        var reflexivo = ParsearCom(_catalogoReflexivo, "01", valorBruto);

        gerado.IndCta.Should().Be(default);
        gerado.ErrosDeFormato.Should().ContainSingle()
            .Which.Mensagem.Should().Be(mensagemEsperada);
        reflexivo.IndCta.Should().Be(gerado.IndCta);
        reflexivo.ErrosDeFormato.Should().Equal(gerado.ErrosDeFormato);
    }

    private static RegistroC050 ParsearCom(
        IRegistroSpedCatalogo catalogo,
        string valorNatureza,
        string indicadorTipoConta = "A")
    {
        var resultado = new ParserEcf(catalogo).ParseLinha(
            $"|C050|01012025|{valorNatureza}|{indicadorTipoConta}|4|001.01|001|CAIXA|",
            numeroLinha: 37);

        resultado.Sucesso.Should().BeTrue();
        return resultado.Valor.Should().BeOfType<RegistroC050>().Which;
    }
}
