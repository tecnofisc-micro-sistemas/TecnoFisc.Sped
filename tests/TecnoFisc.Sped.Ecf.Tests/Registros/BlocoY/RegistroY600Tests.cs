using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY600Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY600()
    {
        AssertRegistroEcf.CodesAreImplemented("Y600");
    }

    [Fact]
    public void DominiosFechados_CoincidemComTabelasCompletasDoManual()
    {
        ValoresSped<TipoQualificacaoSocio>().Should().Equal("PF", "PJ", "FI");
        ValoresSped<QualificacaoSocio>().Should().Equal(
            "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18");
        ValoresSped<QualificacaoRepresentanteLegal>().Should()
            .Equal("01", "02", "03", "04", "05", "06");
    }

    [Theory]
    [InlineData(nameof(RegistroY600.DtAltSoc), typeof(DateOnly), true)]
    [InlineData(nameof(RegistroY600.DtFimSoc), typeof(DateOnly?), false)]
    public void DatasSocietarias_UsamDateOnlyEFormatoExato(
        string nomePropriedade,
        Type tipo,
        bool obrigatorio)
    {
        PropertyInfo propriedade = typeof(RegistroY600).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        propriedade.PropertyType.Should().Be(tipo);
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
        campo.Obrigatorio.Should().Be(obrigatorio);
    }

    [Fact]
    public void Parser_LeDocumentoCompostoLosslessDominiosDatasPercentuaisEValores()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y600|01012012|31122025|105|PF|00000000000|SOCIO TESTE|01|60,1250|40,0000|52998224725|01|100000,00|10000,00|5000,00|3000,00|9000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY600>().Which;
        registro.DtAltSoc.Should().Be(new DateOnly(2012, 1, 1));
        registro.DtFimSoc.Should().Be(new DateOnly(2025, 12, 31));
        registro.Pais.Should().Be("105");
        registro.IndQualif.Should().Be(TipoQualificacaoSocio.PessoaFisica);
        registro.CpfCnpj.Should().Be("00000000000");
        registro.Qualif.Should().Be(QualificacaoSocio.AcionistaPessoaFisicaBrasil);
        registro.PercCapTot.Should().Be(60.1250m);
        registro.PercCapVot.Should().Be(40m);
        registro.CpfRepLeg.Should().Be(Cpf.Create("52998224725"));
        registro.QualifRepLeg.Should().Be(QualificacaoRepresentanteLegal.Procurador);
        registro.VlRemTrab.Should().Be(100000m);
        registro.VlIrRet.Should().Be(9000m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OptionaisVazios_PreservaNulosSemAplicarRegrasTributarias()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y600|01012012||249|PJ||SOCIO EXTERIOR|07|0,0000|0,0000|||0,00|0,00|0,00|0,00|0,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY600>().Which;
        registro.DtFimSoc.Should().BeNull();
        registro.CpfCnpj.Should().BeNull();
        registro.CpfRepLeg.Should().BeNull();
        registro.QualifRepLeg.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DatasDominiosCpfEValoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y600|20250101|31132025|105|XX|00000000000|SOCIO|99|TOTAL|VOTANTE|INVALIDO|99|TRAB|LUC|JUR|REND|IR|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY600>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "DT_ALT_SOC",
                "DT_FIM_SOC",
                "IND_QUALIF",
                "QUALIF",
                "PERC_CAP_TOT",
                "PERC_CAP_VOT",
                "CPF_REP_LEG",
                "QUALIF_REP_LEG",
                "VL_REM_TRAB",
                "VL_LUC_DIV",
                "VL_JUR_CAP",
                "VL_DEM_REND",
                "VL_IR_RET",
            ]);
    }

    private static string[] ValoresSped<TEnum>() where TEnum : struct, Enum
        => typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .ToArray();
}
