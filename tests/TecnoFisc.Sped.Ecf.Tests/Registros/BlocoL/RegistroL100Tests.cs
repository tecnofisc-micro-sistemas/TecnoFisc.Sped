using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL100Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL100(), "L100", "0:N");
    }

    [Fact]
    public void CampoCodigo_UsaAliasNormativoSemColidirComCodigoDoRegistro()
    {
        var gerado = new CatalogoSpedGerado();
        var reflexivo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroL100).Assembly);
        gerado.TentarObter("L100", out var metaGerado).Should().BeTrue();
        reflexivo.TentarObter("L100", out var metaReflexivo).Should().BeTrue();

        metaGerado!.Campos[0].Nome.Should().Be("CODIGO");
        metaReflexivo!.Campos[0].Nome.Should().Be("CODIGO");
        typeof(RegistroL100).GetProperty(nameof(RegistroL100.Codigo)).Should().NotBeNull();
        typeof(RegistroL100).GetProperty(nameof(RegistroL100.CampoCodigo)).Should().NotBeNull();
    }

    [Fact]
    public void Parser_LeMetadadosSaldosEIndicadoresEPreservaCodigosComZeros()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|L100|02.03.04.01.99|CONTA PATRIMONIAL|A|5|03|02.03.04.01|10000,00|C|5000,25|15000,75|00020000,50|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL100>().Which;
        registro.CampoCodigo.Should().Be("02.03.04.01.99");
        registro.Descricao.Should().Be("CONTA PATRIMONIAL");
        registro.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(5);
        registro.CodNat.Should().Be("03");
        registro.CodCtaSup.Should().Be("02.03.04.01");
        registro.ValCtaRefIni.Should().Be(10000m);
        registro.IndValCtaRefIni.Should().Be(IndicadorDebitoCredito.Credor);
        registro.ValCtaRefDeb.Should().Be(5000.25m);
        registro.ValCtaRefCred.Should().Be(15000.75m);
        registro.ValCtaRefFin.Should().Be("00020000,50");
        registro.IndValCtaRefFin.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Fact]
    public void Parser_CamposDescritivosOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|L100|000001||S|||000000|0,00|D|0,00|0,00|0,00|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL100>().Which;
        registro.Descricao.Should().BeNull();
        registro.Nivel.Should().BeNull();
        registro.CodNat.Should().BeNull();
    }

    [Fact]
    public void Parser_SaldoInicialInvalido_RegistraErroMasSaldoFinalTipoCPermaneceLossless()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|L100|000001||S|||000000|INVALIDO|D|0,00|0,00|CALCULADO|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL100>().Which;
        registro.ValCtaRefFin.Should().Be("CALCULADO");
        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "VAL_CTA_REF_INI");
    }
}
