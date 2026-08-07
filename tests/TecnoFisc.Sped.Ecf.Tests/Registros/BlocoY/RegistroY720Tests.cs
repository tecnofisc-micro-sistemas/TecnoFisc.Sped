using System.Reflection;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY720Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY720()
    {
        AssertRegistroEcf.CodesAreImplemented("Y720");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY720(), "Y720", "0:1");
        var campo = typeof(RegistroY720).GetProperty(nameof(RegistroY720.DtLucLiq))!
            .GetCustomAttribute<CampoSpedAttribute>()!;
        campo.Formato.Should().Be("ddMMyyyy");
    }

    [Fact]
    public void Parser_LeValoresAssinadosDataEIndicadores()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y720|-100000,25|31122021|100000000,50|S|N|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY720>().Which;
        registro.LucLiq.Should().Be(-100000.25m);
        registro.DtLucLiq.Should().Be(new DateOnly(2021, 12, 31));
        registro.RecBrutAnt.Should().Be(100000000.50m);
        registro.Intimacao.Should().Be(IndicadorSimNao.Sim);
        registro.IntAtraso.Should().Be(IndicadorSimNao.Nao);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_IndicadorOpcionalVazio_PreservaNulo()
    {
        var registro = new ParserEcf().ParseLinha("|Y720|0,00|31122021|0,00|N||").Valor
            .Should().BeOfType<RegistroY720>().Which;

        registro.IntAtraso.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ValoresDataEIndicadoresInvalidos_RegistramErrosDeFormato()
    {
        var registro = new ParserEcf().ParseLinha(
            "|Y720|LUCRO|20211231|RECEITA|X|X|").Valor
            .Should().BeOfType<RegistroY720>().Which;

        registro.ErrosDeFormato.Select(erro => erro.Campo).Should().Contain([
            "LUC_LIQ", "DT_LUC_LIQ",
            "REC_BRUT_ANT", "INTIMACAO",
            "INT_ATRASO",
        ]);
    }
}
