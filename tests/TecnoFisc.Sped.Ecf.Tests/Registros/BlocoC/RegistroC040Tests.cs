using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC040Tests
{
    private const string LinhaCompleta =
        "|C040|0123456789ABCDEF0123456789ABCDEF01234567|01012025|31122025|1|" +
        "11222333000181|7|00123456789|LIVRO DIARIO|9.00|G|S|N|0|1|01|";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC040(), "C040", "0:12");
    }

    [Fact]
    public void Parser_LeDatasCnpjContagemERepresentacoesTextuais()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC040>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 12, 31));
        registro.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        registro.NumOrd.Should().Be(7);
        registro.Nire.Should().Be("00123456789");
        registro.IdentMf.Should().Be(IndicadorSimNao.Sim);
        registro.IndEscCons.Should().Be(IndicadorSimNao.Nao);
        registro.IndCentralizada.Should().Be("0");
        registro.IndMudancPc.Should().Be("1");
        registro.CodPlanRef.Should().Be("01");
    }

    [Fact]
    public void Parser_CamposFortesInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|C040|HASH|INVALIDA|INVALIDA||INVALIDO|INVALIDO|||||S|N|0|1||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC040>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["DtIni", "DtFin", "Cnpj", "NumOrd"]);
    }
}
